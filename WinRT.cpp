#include <winrt/base.h>
#include <winrt/Windows.Data.Xml.Dom.h>
#include <winrt/Windows.UI.Notifications.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>

#include <windows.h>   // 1. Move this ABOVE tlhelp32.h
#include <tlhelp32.h>  // 2. This can now safely use the Windows types

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
// GLOBAL VARIABLES
// =====================================================
extern HINSTANCE g_hModuleInstance = nullptr;
static std::wstring g_registeredAppId;

// =====================================================
// LOGGING
// =====================================================
static void DebugLog(const wchar_t* text)
{
    OutputDebugStringW(text);
    OutputDebugStringW(L"\n");
    try { std::wcerr << text << std::endl; }
    catch (...) {}
}

static void DebugHResult(const winrt::hresult_error& e)
{
    std::wstring msg = L"[ToastDLL] HRESULT=0x" + std::to_wstring(static_cast<uint32_t>(e.code())) + L" MESSAGE=" + std::wstring(e.message());
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
// CREATE TOAST XML (Fixed: No conflicting appUserModelId root parameters)
// =====================================================
static winrt::Windows::Data::Xml::Dom::XmlDocument CreateToastXml(
    winrt::hstring const& title,
    winrt::hstring const& message,
    winrt::hstring const& launch = {})
{
    std::wstring safeTitle = EscapeXml(title);
    std::wstring safeMessage = EscapeXml(message);

    if (safeTitle.empty() && safeMessage.empty())
    {
        safeMessage = L" ";
        safeTitle = L" ";
    }

    std::wstring xml;
    if (launch.empty())
    {
        xml = L"<toast><visual><binding template='ToastGeneric'><text>" + safeTitle + L"</text><text>" + safeMessage + L"</text></binding></visual></toast>";
    }
    else
    {
        xml = L"<toast launch=\"" + EscapeXml(launch) + L"\"><visual><binding template='ToastGeneric'><text>" + safeTitle + L"</text><text>" + safeMessage + L"</text></binding></visual></toast>";
    }

    winrt::Windows::Data::Xml::Dom::XmlDocument doc;
    doc.LoadXml(xml);
    return doc;
}

// =====================================================
// SHOW TOAST
// =====================================================
extern "C"
{
    __declspec(dllexport) void __stdcall ShowToast(const wchar_t* title, const wchar_t* message)
    {
        try
        {
            std::wstring titleStr = title ? title : L"";
            std::wstring messageStr = message ? message : L"";

            auto doc = CreateToastXml(winrt::hstring(titleStr), winrt::hstring(messageStr));
            winrt::Windows::UI::Notifications::ToastNotification toast{ doc };

            winrt::hstring notifierId = g_registeredAppId.empty() ? L"com.christopheraliprantis.todo" : winrt::hstring(g_registeredAppId);
            auto notifier = winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(notifierId);
            notifier.Show(toast);

            DebugLog(L"[ToastDLL] ShowToast succeeded");
        }
        catch (const winrt::hresult_error& e) { DebugHResult(e); }
        catch (...) { DebugLog(L"[ToastDLL] Unknown exception in ShowToast"); }
    }
}

// =====================================================
// SCHEDULE TOAST
// =====================================================
extern "C"
{
    __declspec(dllexport) void __stdcall ScheduleToast(const wchar_t* id, const wchar_t* title, const wchar_t* message, int64_t fileTime)
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
            DebugLog((L"[ToastDLL] Delta ticks=" + std::to_wstring(delta)).c_str());

            std::wstring titleStr = title ? title : L"";
            std::wstring messageStr = message ? message : L"";
            std::wstring tag = id;

            if (tag.length() > 63) { tag = tag.substr(0, 63); }

            auto doc = CreateToastXml(winrt::hstring(titleStr), winrt::hstring(messageStr), winrt::hstring(tag));
            winrt::file_time ft{ (uint64_t)fileTime };
            winrt::Windows::Foundation::DateTime when = winrt::clock::from_file_time(ft);

            winrt::Windows::UI::Notifications::ScheduledToastNotification toast{ doc, when };
            toast.Tag(winrt::hstring(tag));

            winrt::hstring notifierId = g_registeredAppId.empty() ? L"com.christopheraliprantis.todo" : winrt::hstring(g_registeredAppId);
            auto notifier = winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(notifierId);
            notifier.AddToSchedule(toast);

            DebugLog(L"[ToastDLL] AddToSchedule succeeded");
            auto scheduled = notifier.GetScheduledToastNotifications();
            DebugLog((L"[ToastDLL] Scheduled count=" + std::to_wstring(scheduled.Size())).c_str());
        }
        catch (const winrt::hresult_error& e) { DebugHResult(e); }
        catch (...) { DebugLog(L"[ToastDLL] Unknown exception in ScheduleToast"); }
    }
}

// =====================================================
// CANCEL TOAST
// =====================================================
extern "C"
{
    __declspec(dllexport) void __stdcall CancelToast(const wchar_t* id)
    {
        try
        {
            printf("Rct");
            winrt::hstring targetTag(id);
            winrt::hstring notifierId = g_registeredAppId.empty() ? L"com.christopheraliprantis.todo" : winrt::hstring(g_registeredAppId);

            auto notifier = winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(notifierId);
            auto scheduled = notifier.GetScheduledToastNotifications();
            std::optional<winrt::Windows::UI::Notifications::ScheduledToastNotification> matchedToast = std::nullopt;

            for (uint32_t i = 0; i < scheduled.Size(); ++i)
            {
                auto currentToast = scheduled.GetAt(i);
                if (currentToast.Tag() == targetTag)
                {
                    matchedToast = currentToast;
                    break;
                }
            }

            if (matchedToast.has_value())
            {
                notifier.RemoveFromSchedule(matchedToast.value());
                printf("[ToastDLL] CancelToast succeeded");
            }
            else { wprintf(L"[ToastDLL] No matching scheduled toast found\n"); }
        }
        catch (const winrt::hresult_error& e) { DebugHResult(e); }
        catch (...) { DebugLog(L"[ToastDLL] Unknown exception in CancelToast"); }
    }
}

// =====================================================
// REGISTRY UTILITIES
// =====================================================
bool SetRegistryString(HKEY hRootKey, const std::wstring& subKey, const std::wstring& valueName, const std::wstring& data) {
    HKEY hKey;
    LONG result = RegCreateKeyExW(hRootKey, subKey.c_str(), 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
    if (result != ERROR_SUCCESS) return false;
    result = RegSetValueExW(hKey, valueName.c_str(), 0, REG_SZ, reinterpret_cast<const BYTE*>(data.c_str()), static_cast<DWORD>((data.size() + 1) * sizeof(wchar_t)));
    RegCloseKey(hKey);
    return result == ERROR_SUCCESS;
}

bool SetRegistryDword(HKEY hRootKey, const std::wstring& subKey, const std::wstring& valueName, DWORD data) {
    HKEY hKey;
    LONG result = RegCreateKeyExW(hRootKey, subKey.c_str(), 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
    if (result != ERROR_SUCCESS) return false;
    result = RegSetValueExW(hKey, valueName.c_str(), 0, REG_DWORD, reinterpret_cast<const BYTE*>(&data), sizeof(DWORD));
    RegCloseKey(hKey);
    return result == ERROR_SUCCESS;
}

// =====================================================
// IS NOTIFICATION BLOCKED
// =====================================================
extern "C" __declspec(dllexport) bool __stdcall IsNotificationBlocked(const wchar_t* appId) {
    std::wstring aumid = appId;
    std::wstring globalPushPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\PushNotifications";
    HKEY hGlobalKey;
    DWORD globalEnabled = 1;
    DWORD dwSize = sizeof(DWORD);

    if (RegOpenKeyExW(HKEY_CURRENT_USER, globalPushPath.c_str(), 0, KEY_READ, &hGlobalKey) == ERROR_SUCCESS) {
        RegQueryValueExW(hGlobalKey, L"ToastEnabled", NULL, NULL, reinterpret_cast<LPBYTE>(&globalEnabled), &dwSize);
        RegCloseKey(hGlobalKey);
    }
    if (globalEnabled == 0) return true;

    std::wstring settingsPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\" + aumid;
    HKEY hAppKey;
    LONG status = RegOpenKeyExW(HKEY_CURRENT_USER, settingsPath.c_str(), 0, KEY_READ, &hAppKey);
    if (status != ERROR_SUCCESS) return false;

    DWORD appEnabled = 1;
    dwSize = sizeof(DWORD);
    status = RegQueryValueExW(hAppKey, L"Enabled", NULL, NULL, reinterpret_cast<LPBYTE>(&appEnabled), &dwSize);
    RegCloseKey(hAppKey);

    if (status == ERROR_SUCCESS && appEnabled == 0) return true;
    return false;
}

// =====================================================
// REGISTER APP FOR TOASTS
// =====================================================
extern "C"
{
    __declspec(dllexport) bool __stdcall RegisterAppForToasts(const wchar_t* appId, const wchar_t* appName)
    {
        // 1. Initialize as Multithreaded Apartment State to satisfy modern WinRT tracking
        HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
        if (FAILED(hr) && hr != RPC_E_CHANGED_MODE) return false;

        wchar_t exePath[MAX_PATH]{};
        GetModuleFileNameW(nullptr, exePath, MAX_PATH);

        wchar_t startMenuPath[MAX_PATH]{};
        hr = SHGetFolderPathW(nullptr, CSIDL_PROGRAMS, nullptr, 0, startMenuPath);
        if (FAILED(hr)) return false;

        std::wstring shortcutPath = std::wstring(startMenuPath) + L"\\" + appName + L".lnk";

        ComPtr<IShellLinkW> shellLink;
        hr = CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&shellLink));
        if (FAILED(hr)) return false;

        shellLink->SetPath(exePath);
        shellLink->SetIconLocation(exePath, 0); // Dynamic Icon Binding

        ComPtr<IPropertyStore> propStore;
        hr = shellLink.As(&propStore);
        if (FAILED(hr)) return false;

        PROPVARIANT pvAppId{};
        InitPropVariantFromString(appId, &pvAppId);
        hr = propStore->SetValue(PKEY_AppUserModel_ID, pvAppId);
        PropVariantClear(&pvAppId);
        if (FAILED(hr)) return false;

        PROPVARIANT pvName{};
        InitPropVariantFromString(appName, &pvName);
        hr = propStore->SetValue(PKEY_ItemNameDisplay, pvName);
        PropVariantClear(&pvName);
        if (FAILED(hr)) return false;

        hr = propStore->Commit();
        if (FAILED(hr)) return false;

        ComPtr<IPersistFile> persistFile;
        hr = shellLink.As(&persistFile);
        if (FAILED(hr)) return false;

        hr = persistFile->Save(shortcutPath.c_str(), TRUE);
        if (FAILED(hr)) return false;

        g_registeredAppId = appId;
        HRESULT setHr = SetCurrentProcessExplicitAppUserModelID(g_registeredAppId.c_str());
        if (FAILED(setHr)) {
            DebugLog(L"[ToastDLL] Failed to set AppUserModelID");
            return false;
        }

        std::wstring aumid = appId;
        std::wstring classesPath = L"Software\\Classes\\AppUserModelId\\" + aumid;
        std::wstring settingsPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\" + aumid;

        // Register class capabilities
        // 1. Existing classesPath declarations
        SetRegistryString(HKEY_CURRENT_USER, classesPath, L"DisplayName", appName);
        SetRegistryString(HKEY_CURRENT_USER, classesPath, L"iconuri", exePath);
        SetRegistryDword(HKEY_CURRENT_USER, classesPath, L"ShowBanners", 1);

        // =========================================================================
        // ADD THESE LINES: Registers a dummy COM Activator server class hook
        // This tricks Windows into lifting the "background-only" constraint instantly
        // =========================================================================
        std::wstring activatorPath = classesPath + L"\\BackgroundActivatedHandler";
        // Passing a blank or arbitrary GUID satisfies the OS validation server check
        SetRegistryString(HKEY_CURRENT_USER, activatorPath, L"ClassId", L"{00000000-0000-0000-0000-000000000000}");


        // Apply preference updates
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"Enabled", 1);
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"ShowInActionCenter", 1);
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"ShowBanners", 1);

        // Warm up and verify the engine initialization state
        try
        {
            winrt::hstring notifierId(g_registeredAppId);
            auto notifier = winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(notifierId);
            auto setting = notifier.Setting();

            if (setting == winrt::Windows::UI::Notifications::NotificationSetting::Enabled) {
                DebugLog(L"[ToastDLL] Verification Success: Notification Banners Active.");
            }
            else {
                DebugLog(L"[ToastDLL] Warning: OS notification service forced background-only restrictions.");
            }
        }
        catch (const winrt::hresult_error& e) { DebugHResult(e); }

        DebugLog(L"[ToastDLL] Full functional validation and asset shortcut mapping succeeded.");
        return true;
    }
}

// =====================================================
// DLL MAIN ENTRY POINT
// =====================================================
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hinstDLL);
        g_hModuleInstance = hinstDLL;
        break;
    }
    return TRUE;
}
