#include <winrt/base.h>

#include <winrt/Windows.Data.Xml.Dom.h>
#include <winrt/Windows.UI.Notifications.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>

#include <windows.h>
#include <shobjidl.h>
#include <shlobj.h>
#include <propvarutil.h>
#include <propkey.h>
#include <wrl/client.h>
#include <combaseapi.h>
#include <filesystem>
#include <string>
#include <cstdio>
#include <iostream>
#include <optional>

#pragma comment(lib, "runtimeobject.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "windowsapp.lib")

using namespace Microsoft::WRL;

// =====================================================
// APP ID
// =====================================================
extern HINSTANCE g_hModuleInstance = nullptr;
static constexpr wchar_t APP_ID[] =
L"com.christopheraliprantis.todo";

static std::wstring g_registeredAppId;

// =====================================================
// LOGGING
// =====================================================

static void DebugLog(const wchar_t* text)
{
    OutputDebugStringW(text);
    OutputDebugStringW(L"\n");

    try
    {
        std::wcerr << text << std::endl;
    }
    catch (...)
    {
    }
}

static void DebugHResult(const winrt::hresult_error& e)
{
    std::wstring msg =
        L"[ToastDLL] HRESULT=0x" +
        std::to_wstring(static_cast<uint32_t>(e.code())) +
        L" MESSAGE=" +
        std::wstring(e.message());

    DebugLog(msg.c_str());
}

// =====================================================
// XML ESCAPE
// =====================================================

static std::wstring EscapeXml(winrt::hstring const& s)
{
    std::wstring in = s.c_str();
    std::wstring out;

    for (wchar_t c : in)
    {
        switch (c)
        {
        case L'&':  out += L"&amp;";  break;
        case L'<':  out += L"&lt;";   break;
        case L'>':  out += L"&gt;";   break;
        case L'\'': out += L"&apos;"; break;
        case L'"':  out += L"&quot;"; break;
        default:    out += c;         break;
        }
    }

    return out;
}

// =====================================================
// CREATE TOAST XML
// =====================================================

static winrt::Windows::Data::Xml::Dom::XmlDocument CreateToastXml(
    winrt::hstring const& title,
    winrt::hstring const& message,
    winrt::hstring const& launch = {})
{
    std::wstring safeTitle = EscapeXml(title);
    std::wstring safeMessage = EscapeXml(message);

    // Windows does not like completely empty toast text
    if (safeTitle.empty() && safeMessage.empty())
    {
        safeMessage = L" ";
		safeTitle = L" ";
    }

    std::wstring xml;

    if (launch.empty())
    {
        xml =
            L"<toast>"
            L"<visual>"
            L"<binding template='ToastGeneric'>"
            L"<text>" + safeTitle + L"</text>"
            L"<text>" + safeMessage + L"</text>"
            L"</binding>"
            L"</visual>"
            L"</toast>";
    }
    else
    {
        xml =
            L"<toast launch=\"" + EscapeXml(launch) + L"\">"
            L"<visual>"
            L"<binding template='ToastGeneric'>"
            L"<text>" + safeTitle + L"</text>"
            L"<text>" + safeMessage + L"</text>"
            L"</binding>"
            L"</visual>"
            L"</toast>";
    }

    DebugLog((L"[ToastDLL] XML=" + xml).c_str());

    winrt::Windows::Data::Xml::Dom::XmlDocument doc;
    doc.LoadXml(xml);

    return doc;
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
            std::wstring titleStr =
                title
                ? title
                : L"";

            std::wstring messageStr =
                message
                ? message
                : L"";

            auto doc =
                CreateToastXml(
                    winrt::hstring(titleStr),
                    winrt::hstring(messageStr));

            winrt::Windows::UI::Notifications::ToastNotification toast{ doc };

            winrt::hstring notifierId =
                g_registeredAppId.empty()
                ? winrt::hstring(APP_ID)
                : winrt::hstring(g_registeredAppId);

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier(notifierId);

            notifier.Show(toast);

            DebugLog(L"[ToastDLL] ShowToast succeeded");
        }
        catch (const winrt::hresult_error& e)
        {
            DebugHResult(e);
        }
        catch (...)
        {
            DebugLog(L"[ToastDLL] Unknown exception in ShowToast");
        }
    }
}

// =====================================================
// SCHEDULE TOAST
// =====================================================

extern "C"
{
    __declspec(dllexport)
        void __stdcall ScheduleToast(
            const wchar_t* id,
            const wchar_t* title,
            const wchar_t* message,
            int64_t fileTime)
    {
        try
        {
            FILETIME ftNow{};
            GetSystemTimeAsFileTime(&ftNow);

            ULARGE_INTEGER uli{};
            uli.LowPart = ftNow.dwLowDateTime;
            uli.HighPart = ftNow.dwHighDateTime;

            uint64_t nowTicks = uli.QuadPart;

            if ((uint64_t)fileTime <= nowTicks)
            {
                DebugLog(L"[ToastDLL] Scheduled time is in the past");
                return;
            }

            uint64_t delta = (uint64_t)fileTime - nowTicks;

            DebugLog(
                (L"[ToastDLL] Delta ticks=" +
                    std::to_wstring(delta)).c_str());

            std::wstring titleStr =
                title
                ? title
                : L"";

            std::wstring messageStr =
                message
                ? message
                : L"";

            std::wstring tag = id;
   

            if (tag.length() > 63)
            {
                tag = tag.substr(0, 63);
            }

            auto doc =
                CreateToastXml(
                    winrt::hstring(titleStr),
                    winrt::hstring(messageStr),
                    winrt::hstring(tag));

            winrt::file_time ft{ (uint64_t)fileTime };

            winrt::Windows::Foundation::DateTime when = winrt::clock::from_file_time(ft);


            winrt::Windows::UI::Notifications::ScheduledToastNotification toast
            {
                doc,
                when
            };

            toast.Tag(winrt::hstring(tag));

            winrt::hstring notifierId =
                g_registeredAppId.empty()
                ? winrt::hstring(APP_ID)
                : winrt::hstring(g_registeredAppId);

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier(notifierId);

            notifier.AddToSchedule(toast);

            DebugLog(L"[ToastDLL] AddToSchedule succeeded");

            auto scheduled =
                notifier.GetScheduledToastNotifications();

            DebugLog(
                (L"[ToastDLL] Scheduled count=" +
                    std::to_wstring(scheduled.Size())).c_str());
        }
        catch (const winrt::hresult_error& e)
        {
            DebugHResult(e);
        }
        catch (...)
        {
            DebugLog(L"[ToastDLL] Unknown exception in ScheduleToast");
        }
    }
}

// =====================================================
// CANCEL TOAST
// =====================================================

extern "C"
{
    __declspec(dllexport)
        void __stdcall CancelToast(
            const wchar_t* id)
    {
        try
        {
            printf("Rct");
            winrt::hstring targetTag(id);
            winrt::hstring notifierId{ APP_ID };

            auto notifier =
                winrt::Windows::UI::Notifications::ToastNotificationManager
                ::CreateToastNotifier(notifierId);

            auto scheduled =
                notifier.GetScheduledToastNotifications();

            // 1. Initialize it as empty using std::nullopt
            std::optional<winrt::Windows::UI::Notifications::ScheduledToastNotification> matchedToast = std::nullopt;

            for (uint32_t i = 0; i < scheduled.Size(); ++i)
            {
                auto currentToast = scheduled.GetAt(i);

                if (currentToast.Tag() == targetTag)
                {
                    // 2. Save the match and break immediately
                    matchedToast = currentToast;
                    break;
                }
            }

            // 3. Check if the optional actually holds a value before using it
            if (matchedToast.has_value())
            {
                notifier.RemoveFromSchedule(matchedToast.value());
                printf("[ToastDLL] CancelToast succeeded");
            }
            else
            {
                winrt::hstring m = L"[ToastDLL] No matching scheduled toast found";
                wprintf(L"%s\n", m.c_str());
            }
        }
        catch (const winrt::hresult_error& e)
        {
            DebugHResult(e);
        }
        catch (...)
        {
            DebugLog(L"[ToastDLL] Unknown exception in CancelToast");
        }
    }
}


// =====================================================
// REGISTER APP FOR TOASTS
// =====================================================
bool SetRegistryString(HKEY hRootKey, const std::wstring& subKey, const std::wstring& valueName, const std::wstring& data) {
    HKEY hKey;
    LONG result = RegCreateKeyExW(hRootKey, subKey.c_str(), 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
    if (result != ERROR_SUCCESS) return false;

    result = RegSetValueExW(hKey, valueName.c_str(), 0, REG_SZ, reinterpret_cast<const BYTE*>(data.c_str()), static_cast<DWORD>((data.size() + 1) * sizeof(wchar_t)));
    RegCloseKey(hKey);
    return result == ERROR_SUCCESS;
}

// Helper to set DWORD values in the registry
bool SetRegistryDword(HKEY hRootKey, const std::wstring& subKey, const std::wstring& valueName, DWORD data) {
    HKEY hKey;
    LONG result = RegCreateKeyExW(hRootKey, subKey.c_str(), 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
    if (result != ERROR_SUCCESS) return false;

    result = RegSetValueExW(hKey, valueName.c_str(), 0, REG_DWORD, reinterpret_cast<const BYTE*>(&data), sizeof(DWORD));
    RegCloseKey(hKey);
    return result == ERROR_SUCCESS;
}

std::wstring GetAbsoluteIconPath() {
    wchar_t buffer[MAX_PATH];

    // 1. Get the path to winrtapis.dll 
    // (e.g., C:\YourProject\DLLs\winrtapis.dll)
    DWORD length = GetModuleFileNameW(g_hModuleInstance, buffer, MAX_PATH);
    if (length == 0) return L"";

    std::filesystem::path dllFilePath(buffer);

    // 2. .parent_path() gets the "DLLs" folder
    // 3. .parent_path() again gets the project root folder (parent of DLLs)
    std::filesystem::path projectRoot = dllFilePath.parent_path().parent_path();

    // 4. Navigate forward: Root -> Assets -> Icons -> todoico.ico
    std::filesystem::path iconPath = projectRoot / L"Assets" / L"Icons" / L"todoico.ico";

    return iconPath.wstring();
}

extern "C"
{


    __declspec(dllexport)
        bool __stdcall RegisterAppForToasts(
            const wchar_t* appId,
            const wchar_t* appName)
    {
        HRESULT hr =
            CoInitializeEx(
                nullptr,
                COINIT_APARTMENTTHREADED);

        if (FAILED(hr) && hr != RPC_E_CHANGED_MODE)
        {
            return false;
        }

        wchar_t exePath[MAX_PATH]{};

        GetModuleFileNameW(
            nullptr,
            exePath,
            MAX_PATH);

        wchar_t startMenuPath[MAX_PATH]{};

        hr =
            SHGetFolderPathW(
                nullptr,
                CSIDL_PROGRAMS,
                nullptr,
                0,
                startMenuPath);

        if (FAILED(hr))
        {
            return false;
        }

        std::wstring shortcutPath =
            std::wstring(startMenuPath) +
            L"\\" +
            appName +
            L".lnk";

        ComPtr<IShellLinkW> shellLink;

        hr =
            CoCreateInstance(
                CLSID_ShellLink,
                nullptr,
                CLSCTX_INPROC_SERVER,
                IID_PPV_ARGS(&shellLink));

        if (FAILED(hr))
        {
            return false;
        }

        shellLink->SetPath(exePath);

        ComPtr<IPropertyStore> propStore;

        hr = shellLink.As(&propStore);

        if (FAILED(hr))
        {
            return false;
        }

        PROPVARIANT pv{};

        InitPropVariantFromString(
            appId,
            &pv);

        hr =
            propStore->SetValue(
                PKEY_AppUserModel_ID,
                pv);

        PropVariantClear(&pv);

        if (FAILED(hr))
        {
            return false;
        }

        hr = propStore->Commit();

        if (FAILED(hr))
        {
            return false;
        }

        ComPtr<IPersistFile> persistFile;

        hr = shellLink.As(&persistFile);

        if (FAILED(hr))
        {
            return false;
        }

        hr =
            persistFile->Save(
                shortcutPath.c_str(),
                TRUE);

        if (FAILED(hr))
        {
            return false;
        }

        g_registeredAppId = appId;

        HRESULT setHr =
            SetCurrentProcessExplicitAppUserModelID(appId);

        if (FAILED(setHr))
        {
            DebugLog(L"[ToastDLL] Failed to set AppUserModelID");
        }

        DebugLog(L"[ToastDLL] RegisterAppForToasts succeeded");

        std::wstring aumid = appId;
        std::wstring displayName = L"ToDo";
        std::wstring iconPath = GetAbsoluteIconPath(); // Use the helper function to get the absolute path

        // 2. Paths to the target registry structures
        std::wstring classesPath = L"Software\\Classes\\AppUserModelId\\" + aumid;
        std::wstring settingsPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\" + aumid;

        std::wcout << L"[ToastDLL] Registering AUMID and enabling notification flags..." << std::endl;

        // 3. Populate identity data (Crucial for AppUserModelId mapping)
        bool identityOk = true;
        identityOk &= SetRegistryString(HKEY_CURRENT_USER, classesPath, L"DisplayName", displayName);
        identityOk &= SetRegistryString(HKEY_CURRENT_USER, classesPath, L"IconUri", iconPath);

        // 4. Force notification settings to active/working state 
        bool settingsOk = true;
        settingsOk &= SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"Enabled", 1); // 1 = On, 0 = Off
        settingsOk &= SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"ShowInActionCenter", 1);

        if (identityOk && settingsOk) {
            std::wcout << L"[ToastDLL] Success! Registry configured for AUMID: " << aumid << std::endl;
        }
        else {
            std::wcerr << L"[ToastDLL] Failed to write configuration to registry." << std::endl;
        }
        return true;
    }
}

// =====================================================
// DLL MAIN
// =====================================================

BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,
    DWORD fdwReason,
    LPVOID lpvReserved)
{
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hinstDLL);

        // 2. Assign the handle here
        g_hModuleInstance = hinstDLL;
        break;
    }
    return TRUE;
}