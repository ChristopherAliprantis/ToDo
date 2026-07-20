#include <windows.h>
#include <shobjidl.h>
#include <shlobj.h>
#include <propkey.h>
#include <propvarutil.h>
#include <wrl/client.h>

#include <winrt/base.h>
#include <winrt/Windows.Data.Xml.Dom.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.UI.Notifications.h>

#include <cstdio>
#include <cstdint>
#include <string>
#include <filesystem>
#include <iostream>

#pragma comment(lib, "runtimeobject.lib")
#pragma comment(lib, "windowsapp.lib")
#pragma comment(lib, "shlwapi.lib")


using Microsoft::WRL::ComPtr;

//
// =====================================================
// GLOBALS
// =====================================================
//

static std::wstring g_registeredAppId;

static HINSTANCE g_hModuleInstance = nullptr;


//
// =====================================================
// LOGGING
// =====================================================
//

static void PrintError(const char* where, HRESULT hr)
{
    std::printf(
        "[ToastDLL] %s failed HRESULT=0x%08X\n",
        where,
        static_cast<unsigned int>(hr)
    );
}

static void PrintWinrtError(const char* where, const winrt::hresult_error& e)
{
    std::wcerr
        << L"[ToastDLL] "
        << where
        << L" HRESULT=0x"
        << std::hex
        << static_cast<uint32_t>(e.code())
        << L"\nMessage: "
        << e.message().c_str()
        << L"\n";
}


//
// =====================================================
// WINRT INITIALIZATION
// =====================================================
//



//
// =====================================================
// XML ESCAPING
// =====================================================
//

static std::wstring EscapeXml(const std::wstring& input)
{
    std::wstring output;

    output.reserve(input.size());

    for (wchar_t c : input)
    {
        switch (c)
        {
        case L'&':
            output += L"&amp;";
            break;

        case L'<':
            output += L"&lt;";
            break;

        case L'>':
            output += L"&gt;";
            break;

        case L'"':
            output += L"&quot;";
            break;

        case L'\'':
            output += L"&apos;";
            break;

        default:
            output += c;
            break;
        }
    }

    return output;
}


//
// =====================================================
// SAFE STRING HELPER
// =====================================================
//

static std::wstring SafeString(const wchar_t* value)
{
    if (!value)
        return L"";

    return std::wstring(value);
}

//
// =====================================================
// CREATE TOAST XML
// =====================================================
//

static winrt::Windows::Data::Xml::Dom::XmlDocument CreateToastXml(
    const std::wstring& title,
    const std::wstring& message)
{
    std::wstring safeTitle = EscapeXml(title);
    std::wstring safeMessage = EscapeXml(message);

    if (safeTitle.empty())
        safeTitle = L" ";

    if (safeMessage.empty())
        safeMessage = L" ";

    //
    // No launch attribute.
    // Clicking the toast will not activate anything.
    //
    std::wstring xml =
        L"<toast>"
        L"<visual>"
        L"<binding template=\"ToastGeneric\">"
        L"<text>" + safeTitle + L"</text>"
        L"<text>" + safeMessage + L"</text>"
        L"</binding>"
        L"</visual>"
        L"</toast>";

    winrt::Windows::Data::Xml::Dom::XmlDocument document;

    document.LoadXml(xml);

    return document;
}


//
// =====================================================
// APP USER MODEL ID
// =====================================================
//

static std::wstring CurrentAppId()
{
    if (!g_registeredAppId.empty())
        return g_registeredAppId;

    //
    // Fallback.
    // Must match the shortcut AppUserModelID.
    //
    return L"com.christopheraliprantis.todo";
}


//
// =====================================================
// CREATE TOAST NOTIFIER
// =====================================================
//

static winrt::Windows::UI::Notifications::ToastNotifier CreateNotifier()
{
    auto appId = winrt::hstring(CurrentAppId());

    return winrt::Windows::UI::Notifications::
        ToastNotificationManager::
        CreateToastNotifier(appId);
}


//
// =====================================================
// GET HOST EXECUTABLE PATH
// =====================================================
//

static std::wstring GetHostExecutablePath()
{
    wchar_t buffer[MAX_PATH]{};

    DWORD length =
        GetModuleFileNameW(
            nullptr,
            buffer,
            MAX_PATH
        );

    if (length == 0)
    {
        return {};
    }

    return std::wstring(buffer, length);
}


//
// =====================================================
// GET HOST DIRECTORY
// =====================================================
//

static std::wstring GetHostDirectory()
{
    std::wstring path = GetHostExecutablePath();

    if (path.empty())
        return {};

    auto pos = path.find_last_of(L"\\/");

    if (pos == std::wstring::npos)
        return {};

    return path.substr(0, pos);
}

//
// =====================================================
// REGISTER APP FOR TOASTS
// =====================================================
//

extern "C"
__declspec(dllexport)
bool __stdcall RegisterAppForToasts(
    const wchar_t* appId,
    const wchar_t* appName)
{
    if (!appId || !appName)
    {
        printf("[ToastDLL] RegisterAppForToasts invalid arguments\n");
        return false;
    }

    try
    {
        // Adapt to the host process apartment.
        winrt::init_apartment();

        g_registeredAppId = appId;

        //
        // Get host EXE path (NOT DLL path)
        //
        std::wstring exePath = GetHostExecutablePath();

        if (exePath.empty())
        {
            printf("[ToastDLL] Could not get host executable path\n");
            return false;
        }

        //
        // Set current process AUMID
        //
        HRESULT hr = SetCurrentProcessExplicitAppUserModelID(
            g_registeredAppId.c_str()
        );

        if (FAILED(hr))
        {
            PrintError(
                "SetCurrentProcessExplicitAppUserModelID",
                hr
            );
            return false;
        }


        //
        // Start Menu Programs
        //
        wchar_t programsPath[MAX_PATH]{};

        hr = SHGetFolderPathW(
            nullptr,
            CSIDL_PROGRAMS,
            nullptr,
            SHGFP_TYPE_CURRENT,
            programsPath
        );

        if (FAILED(hr))
        {
            PrintError("SHGetFolderPathW", hr);
            return false;
        }


        std::wstring shortcutPath =
            std::wstring(programsPath) +
            L"\\" +
            appName +
            L".lnk";


        //
        // Always recreate during development so AUMID updates.
        //
        std::filesystem::remove(shortcutPath);


        Microsoft::WRL::ComPtr<IShellLinkW> shellLink;

        hr = CoCreateInstance(
            CLSID_ShellLink,
            nullptr,
            CLSCTX_INPROC_SERVER,
            IID_PPV_ARGS(&shellLink)
        );

        if (FAILED(hr))
        {
            PrintError(
                "CoCreateInstance ShellLink",
                hr
            );
            return false;
        }


        shellLink->SetPath(
            exePath.c_str()
        );

        shellLink->SetWorkingDirectory(
            GetHostDirectory().c_str()
        );

        shellLink->SetIconLocation(
            exePath.c_str(),
            0
        );


        Microsoft::WRL::ComPtr<IPropertyStore> propertyStore;

        hr = shellLink.As(&propertyStore);

        if (FAILED(hr))
        {
            PrintError(
                "Query IPropertyStore",
                hr
            );
            return false;
        }


        PROPVARIANT appIdValue{};
        InitPropVariantFromString(
            appId,
            &appIdValue
        );

        hr = propertyStore->SetValue(
            PKEY_AppUserModel_ID,
            appIdValue
        );

        PropVariantClear(&appIdValue);

        if (FAILED(hr))
        {
            PrintError(
                "Set PKEY_AppUserModel_ID",
                hr
            );
            return false;
        }


        PROPVARIANT nameValue{};
        InitPropVariantFromString(
            appName,
            &nameValue
        );

        hr = propertyStore->SetValue(
            PKEY_ItemNameDisplay,
            nameValue
        );

        PropVariantClear(&nameValue);

        if (FAILED(hr))
        {
            PrintError(
                "Set PKEY_ItemNameDisplay",
                hr
            );
            return false;
        }


        hr = propertyStore->Commit();

        if (FAILED(hr))
        {
            PrintError(
                "PropertyStore Commit",
                hr
            );
            return false;
        }


        Microsoft::WRL::ComPtr<IPersistFile> persistFile;

        hr = shellLink.As(&persistFile);

        if (FAILED(hr))
        {
            PrintError(
                "Query IPersistFile",
                hr
            );
            return false;
        }


        hr = persistFile->Save(
            shortcutPath.c_str(),
            TRUE
        );

        if (FAILED(hr))
        {
            PrintError(
                "Save shortcut",
                hr
            );
            return false;
        }


        printf("[ToastDLL] RegisterAppForToasts succeeded\n");
        return true;
    }
    catch (const winrt::hresult_error& e)
    {
        printf(
            "[ToastDLL] WinRT error: 0x%08X\n",
            (uint32_t)e.code()
        );
        return false;
    }
    catch (...)
    {
        printf("[ToastDLL] Unknown RegisterAppForToasts error\n");
        return false;
    }
}


//
// =====================================================
// SHOW TOAST
// =====================================================
//
extern "C"
{
    __declspec(dllexport)
        void __stdcall ShowToast(
            const wchar_t* title,
            const wchar_t* message)
    {
        try
        {
            winrt::init_apartment();

            auto xml = CreateToastXml(
                title ? title : L"",
                message ? message : L""
            );

            winrt::Windows::UI::Notifications::ToastNotification toast(xml);

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier();

            notifier.Show(toast);

            printf("[ToastDLL] ShowToast succeeded\n");
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] ShowToast HRESULT: 0x%08X\n",
                (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] ShowToast unknown error\n");
        }
    }


    __declspec(dllexport)
        void __stdcall ScheduleToast(
            const wchar_t* id,
            const wchar_t* title,
            const wchar_t* message,
            int64_t fileTime)
    {
        try
        {
            winrt::init_apartment();

            std::wstring tag = id ? id : L"";

            if (tag.size() > 63)
                tag.resize(63);

            auto xml = CreateToastXml(
                title ? title : L"",
                message ? message : L""
            );

            winrt::Windows::Foundation::DateTime when =
                winrt::clock::from_file_time(
                    winrt::file_time((uint64_t)fileTime)
                );

            winrt::Windows::UI::Notifications::ScheduledToastNotification toast(
                xml,
                when
            );

            toast.Tag(tag);

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier();

            notifier.AddToSchedule(toast);

            printf("[ToastDLL] ScheduleToast succeeded\n");
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] ScheduleToast HRESULT: 0x%08X\n",
                (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] ScheduleToast unknown error\n");
        }
    }


    __declspec(dllexport)
        void __stdcall CancelToast(
            const wchar_t* id)
    {
        try
        {
            winrt::init_apartment();

            if (!id)
                return;

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier();

            auto scheduled =
                notifier.GetScheduledToastNotifications();

            winrt::hstring target(id);

            for (uint32_t i = 0; i < scheduled.Size(); i++)
            {
                auto toast = scheduled.GetAt(i);

                if (toast.Tag() == target)
                {
                    notifier.RemoveFromSchedule(toast);
                    printf("[ToastDLL] CancelToast succeeded\n");
                    return;
                }
            }

            printf("[ToastDLL] No matching toast found\n");
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] CancelToast HRESULT: 0x%08X\n",
                (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] CancelToast unknown error\n");
        }
    }
}


extern "C"
{
    __declspec(dllexport)
        bool __stdcall IsNotificationBlocked(const wchar_t* appId)
    {
        printf("[ToastDLL] IsNotificationBlocked called\n");

        HKEY hKey{};
        DWORD enabled = 1;
        DWORD size = sizeof(DWORD);

        const wchar_t* path =
            L"Software\\Microsoft\\Windows\\CurrentVersion\\PushNotifications";

        if (RegOpenKeyExW(
            HKEY_CURRENT_USER,
            path,
            0,
            KEY_READ,
            &hKey) == ERROR_SUCCESS)
        {
            RegQueryValueExW(
                hKey,
                L"ToastEnabled",
                nullptr,
                nullptr,
                reinterpret_cast<LPBYTE>(&enabled),
                &size);

            RegCloseKey(hKey);
        }

        return enabled == 0;
    }


    __declspec(dllexport)
        bool __stdcall IsNotificationDisabled(const wchar_t* appId)
    {
        printf("[ToastDLL] IsNotificationDisabled called\n");

        if (!appId)
            return true;

        std::wstring path =
            L"Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\" +
            std::wstring(appId);

        HKEY hKey{};

        if (RegOpenKeyExW(
            HKEY_CURRENT_USER,
            path.c_str(),
            0,
            KEY_READ,
            &hKey) != ERROR_SUCCESS)
        {
            // No settings key means default enabled
            return false;
        }

        DWORD enabled = 1;
        DWORD size = sizeof(DWORD);

        RegQueryValueExW(
            hKey,
            L"Enabled",
            nullptr,
            nullptr,
            reinterpret_cast<LPBYTE>(&enabled),
            &size);

        RegCloseKey(hKey);

        return enabled == 0;
    }
}
