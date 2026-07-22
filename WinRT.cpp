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
#include <vector>
#include <filesystem>
#include <iostream>

#pragma comment(lib, "runtimeobject.lib")
#pragma comment(lib, "windowsapp.lib")
#pragma comment(lib, "shlwapi.lib")

using Microsoft::WRL::ComPtr;
using namespace winrt;
using namespace winrt::Windows::UI::Notifications;
// =====================================================
// GLOBALS
// =====================================================
static std::wstring g_registeredAppId;
static HINSTANCE g_hModuleInstance = nullptr;

// =====================================================
// LOGGING
// =====================================================
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

// =====================================================
// XML ESCAPING
// =====================================================
static std::wstring EscapeXml(const std::wstring& input)
{
    std::wstring output;
    output.reserve(input.size());

    for (wchar_t c : input)
    {
        switch (c)
        {
        case L'&':  output += L"&amp;";  break;
        case L'<':  output += L"&lt;";   break;
        case L'>':  output += L"&gt;";   break;
        case L'"':  output += L"&quot;"; break;
        case L'\'': output += L"&apos;"; break;
        default:    output += c;         break;
        }
    }
    return output;
}

// =====================================================
// SAFE STRING HELPER
// =====================================================
static std::wstring SafeString(const wchar_t* value)
{
    if (!value)
        return L"";
    return std::wstring(value);
}

// =====================================================
// CREATE TOAST XML
// =====================================================
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

// =====================================================
// APP USER MODEL ID
// =====================================================
static std::wstring CurrentAppId()
{
    if (!g_registeredAppId.empty())
        return g_registeredAppId;

    return L"com.christopheraliprantis.todo";
}

// =====================================================
// CREATE TOAST NOTIFIER
// =====================================================
static winrt::Windows::UI::Notifications::ToastNotifier CreateNotifier()
{
    auto appId = winrt::hstring(CurrentAppId());
    return winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(appId);
}

// =====================================================
// GET HOST EXECUTABLE PATH
// =====================================================
static std::wstring GetHostExecutablePath()
{
    std::vector<wchar_t> buffer(MAX_PATH);
    while (true) {
        DWORD sizeUsed = GetModuleFileNameW(NULL, buffer.data(), static_cast<DWORD>(buffer.size()));
        if (sizeUsed == 0) return L"";
        if (sizeUsed < buffer.size()) return std::wstring(buffer.data(), sizeUsed);
        buffer.resize(buffer.size() * 2);
    }
}

// =====================================================
// GET HOST DIRECTORY
// =====================================================
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

// =====================================================
// REGISTER APP FOR TOASTS
// =====================================================
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
        winrt::init_apartment();
        g_registeredAppId = appId;

        std::wstring exePath = GetHostExecutablePath();
        if (exePath.empty())
        {
            printf("[ToastDLL] Could not get host executable path\n");
            return false;
        }

        HRESULT hr = SetCurrentProcessExplicitAppUserModelID(g_registeredAppId.c_str());
        if (FAILED(hr))
        {
            PrintError("SetCurrentProcessExplicitAppUserModelID", hr);
            return false;
        }

        wchar_t programsPath[MAX_PATH]{};
        hr = SHGetFolderPathW(nullptr, CSIDL_PROGRAMS, nullptr, SHGFP_TYPE_CURRENT, programsPath);
        if (FAILED(hr))
        {
            PrintError("SHGetFolderPathW", hr);
            return false;
        }

        std::wstring shortcutPath = std::wstring(programsPath) + L"\\" + appName + L".lnk";
        std::filesystem::remove(shortcutPath);

        ComPtr<IShellLinkW> shellLink;
        hr = CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&shellLink));
        if (FAILED(hr))
        {
            PrintError("CoCreateInstance ShellLink", hr);
            return false;
        }

        shellLink->SetPath(exePath.c_str());
        shellLink->SetWorkingDirectory(GetHostDirectory().c_str());
        shellLink->SetIconLocation(exePath.c_str(), 0);

        ComPtr<IPropertyStore> propertyStore;
        hr = shellLink.As(&propertyStore);
        if (FAILED(hr))
        {
            PrintError("Query IPropertyStore", hr);
            return false;
        }

        PROPVARIANT appIdValue{};
        InitPropVariantFromString(appId, &appIdValue);
        hr = propertyStore->SetValue(PKEY_AppUserModel_ID, appIdValue);
        PropVariantClear(&appIdValue);
        if (FAILED(hr))
        {
            PrintError("Set PKEY_AppUserModel_ID", hr);
            return false;
        }

        PROPVARIANT nameValue{};
        InitPropVariantFromString(appName, &nameValue);
        hr = propertyStore->SetValue(PKEY_ItemNameDisplay, nameValue);
        PropVariantClear(&nameValue);
        if (FAILED(hr))
        {
            PrintError("Set PKEY_ItemNameDisplay", hr);
            return false;
        }

        hr = propertyStore->Commit();
        if (FAILED(hr))
        {
            PrintError("PropertyStore Commit", hr);
            return false;
        }

        ComPtr<IPersistFile> persistFile;
        hr = shellLink.As(&persistFile);
        if (FAILED(hr))
        {
            PrintError("Query IPersistFile", hr);
            return false;
        }

        hr = persistFile->Save(shortcutPath.c_str(), TRUE);
        if (FAILED(hr))
        {
            PrintError("Save shortcut", hr);
            return false;
        }

        // --- REGISTRY WRITING LOGIC (Opens if exists, creates if missing) ---
        // --- Create/Open notification settings key and overwrite iconuri ---
        std::wstring aumid = appId;
        std::wstring iconUriValue = GetHostDirectory() + L"\\Assets\\Icons\\todoico.ico";
        std::wstring subKey =
            L"Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\" + aumid;

        HKEY hKey = nullptr;
        DWORD disposition = 0;

        LSTATUS status = RegCreateKeyExW(
            HKEY_CURRENT_USER,
            subKey.c_str(),
            0,
            nullptr,
            REG_OPTION_NON_VOLATILE,
            KEY_READ | KEY_WRITE,
            nullptr,
            &hKey,
            &disposition);

        if (status != ERROR_SUCCESS)
        {
            std::wcerr << L"[ToastDLL] RegCreateKeyExW failed. Error: " << status << std::endl;
            return false;
        }

        if (disposition == REG_CREATED_NEW_KEY)
        {
            std::wcout << L"[ToastDLL] Created registry key: " << subKey << std::endl;
        }
        else
        {
            std::wcout << L"[ToastDLL] Opened existing registry key: " << subKey << std::endl;
        }
        std::wcout << L"AUMID: [" << aumid << L"]\n";
        std::wcout << L"SubKey: [" << subKey << L"]\n";
        status = RegSetValueExW(
            hKey,
            L"iconuri",
            0,
            REG_SZ,
            reinterpret_cast<const BYTE*>(iconUriValue.c_str()),
            static_cast<DWORD>((iconUriValue.length() + 1) * sizeof(wchar_t)));

        if (status != ERROR_SUCCESS)
        {
            std::wcerr << L"[ToastDLL] RegSetValueExW failed. Error: " << status << std::endl;
            RegCloseKey(hKey);
            return false;
        }

        RegCloseKey(hKey);

        std::wcout << L"[ToastDLL] iconuri set to: " << iconUriValue << std::endl;

        return true;
	}
	catch (const winrt::hresult_error& e)
	{
		PrintWinrtError("RegisterAppForToasts", e);
		return false;
	}
    catch (...)
    {
        std::printf("[ToastDLL] RegisterAppForToasts unknown error\n");
        return false;
    }
}

// =====================================================
// SHOW TOAST
// =====================================================
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
            auto notifier = CreateNotifier();
            notifier.Show(toast);
            printf("[ToastDLL] ShowToast succeeded\n");
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] ShowToast HRESULT: 0x%08X\n", (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] ShowToast unknown error\n");
        }
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

            winrt::Windows::UI::Notifications::ScheduledToastNotification toast(xml, when);
            toast.Tag(tag);

            auto notifier = CreateNotifier();
            notifier.AddToSchedule(toast);
            printf("[ToastDLL] ScheduleToast succeeded\n");
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] ScheduleToast HRESULT: 0x%08X\n", (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] ScheduleToast unknown error\n");
        }
    }

    __declspec(dllexport)
        void __stdcall CancelToast(const wchar_t* id)
    {
        try
        {
            winrt::init_apartment();
            if (!id)
                return;

            auto notifier = CreateNotifier();
            auto scheduled = notifier.GetScheduledToastNotifications();
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
            printf("[ToastDLL] CancelToast HRESULT: 0x%08X\n", (uint32_t)e.code());
        }
        catch (...)
        {
            printf("[ToastDLL] CancelToast unknown error\n");
        }
    }

    __declspec(dllexport)
        bool __stdcall IsNotificationDisabled(const wchar_t* appId)
    {
        printf("[ToastDLL] IsNotificationDisabled called\n");

        if (!appId)
            return true;

        try
        {
            winrt::init_apartment();

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager::
                CreateToastNotifier(appId);

            auto setting = notifier.Setting();

            // Any state other than Enabled means notifications are disabled
            return setting !=
                winrt::Windows::UI::Notifications::NotificationSetting::Enabled;
        }
        catch (const winrt::hresult_error& e)
        {
            printf("[ToastDLL] IsNotificationDisabled error: %ls\n",
                e.message().c_str());

            // If Windows cannot tell us, treat as disabled
            return true;
        }
    }


    __declspec(dllexport)
        bool __stdcall IsNotificationBlocked(const wchar_t* appId)
    {
        printf("[ToastDLL] IsNotificationBlocked called\n");

        HKEY hKey = nullptr;

        const wchar_t* path =
            L"Software\\Microsoft\\Windows\\CurrentVersion\\PushNotifications";

        DWORD toastEnabled = 1;
        DWORD size = sizeof(DWORD);

        LONG result = RegOpenKeyExW(
            HKEY_CURRENT_USER,
            path,
            0,
            KEY_READ,
            &hKey);

        if (result != ERROR_SUCCESS)
        {
            // Could not read system setting, assume not blocked
            return false;
        }


        result = RegQueryValueExW(
            hKey,
            L"ToastEnabled",
            nullptr,
            nullptr,
            reinterpret_cast<LPBYTE>(&toastEnabled),
            &size);

        RegCloseKey(hKey);


        if (result != ERROR_SUCCESS)
        {
            // Value missing = system notifications are not known to be blocked
            return false;
        }


        printf("[ToastDLL] System ToastEnabled=%lu\n", toastEnabled);

        // 0 = Windows global notifications disabled
        return toastEnabled == 0;
    }