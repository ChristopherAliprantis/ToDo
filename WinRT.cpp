#include <winrt/base.h>
#include <winrt/Windows.Data.Xml.Dom.h>
#include <winrt/Windows.UI.Notifications.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>

#include <windows.h>   
#include <tlhelp32.h>  

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
// REGISTER APP FOR TOASTS
// =====================================================
extern "C"
{
    __declspec(dllexport)
        bool __stdcall RegisterAppForToasts(const wchar_t* appId, const wchar_t* appName)
    {
        // 1. MUST use COINIT_MULTITHREADED for stable runtime shell service tracking
        HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
        if (FAILED(hr) && hr != RPC_E_CHANGED_MODE)
        {
            return false;
        }

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

        // Dynamic Icon Binding: Pulls index 0 from current running executable
        shellLink->SetIconLocation(exePath, 0);

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

        // Populate Software\Classes directory rules
        SetRegistryString(HKEY_CURRENT_USER, classesPath, L"DisplayName", appName);
        SetRegistryString(HKEY_CURRENT_USER, classesPath, L"IconUri", exePath);
        SetRegistryDword(HKEY_CURRENT_USER, classesPath, L"ShowBanners", 1);

        // Populate user choice preferences subkey tree
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"Enabled", 1);
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"ShowInActionCenter", 1);
        SetRegistryDword(HKEY_CURRENT_USER, settingsPath, L"ShowBanners", 1);

        // Verification validation tracking engine lookup
        try
        {
            winrt::hstring notifierId(g_registeredAppId);
            auto notifier = winrt::Windows::UI::Notifications::ToastNotificationManager::CreateToastNotifier(notifierId);
            auto setting = notifier.Setting();

            if (setting == winrt::Windows::UI::Notifications::NotificationSetting::Enabled)
            {
                DebugLog(L"[ToastDLL] Verification Success: Notification Banners Active.");
            }
            else
            {
                DebugLog(L"[ToastDLL] Warning: OS notification service forced background-only restrictions.");
            }
        }
        catch (const winrt::hresult_error& e)
        {
            DebugHResult(e);
        }

        return true;
    }
}

// =====================================================
// MAIN DLL ENTRY POINT
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
