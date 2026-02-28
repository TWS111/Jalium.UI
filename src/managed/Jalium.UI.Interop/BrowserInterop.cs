using System.Runtime.InteropServices;

namespace Jalium.UI.Interop;

internal static class BrowserInterop
{
    private const string BrowserLib = "jalium.native.browser";

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NavigationStartingCallback(nint userData, nint uri, int isRedirected, ref int cancel);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NavigationCompletedCallback(nint userData, int isSuccess, int httpStatusCode);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void SourceChangedCallback(nint userData, int isNewDocument);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void ContentLoadingCallback(nint userData, int isErrorPage);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void DocumentTitleChangedCallback(nint userData, nint title);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void WebMessageReceivedCallback(nint userData, nint message);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NewWindowRequestedCallback(nint userData, nint uri, int isUserInitiated, ref int handled);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void ProcessFailedCallback(nint userData, int processFailedKind);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void ZoomFactorChangedCallback(nint userData, double zoomFactor);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void ScriptCompletedCallback(nint userData, int result, nint resultJson);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_initialize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Initialize();

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_shutdown", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Shutdown();

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_available_browser_version_string", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int GetAvailableBrowserVersionString(string? browserExecutableFolder, out nint version);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_free_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void FreeString(nint value);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_create_environment", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int CreateEnvironment(string? browserExecutableFolder, string? userDataFolder, out nint environment);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_destroy_environment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DestroyEnvironment(nint environment);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_create_controller", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int CreateController(nint environment, nint parentWindow, int useCompositionController, out nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_destroy_controller", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DestroyController(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_callbacks", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetCallbacks(
        nint controller,
        NavigationStartingCallback? navigationStarting,
        NavigationCompletedCallback? navigationCompleted,
        SourceChangedCallback? sourceChanged,
        ContentLoadingCallback? contentLoading,
        DocumentTitleChangedCallback? documentTitleChanged,
        WebMessageReceivedCallback? webMessageReceived,
        NewWindowRequestedCallback? newWindowRequested,
        ProcessFailedCallback? processFailed,
        ZoomFactorChangedCallback? zoomFactorChanged,
        nint userData);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_navigate", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int Navigate(nint controller, string uri);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_navigate_to_string", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int NavigateToString(nint controller, string html);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_reload", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Reload(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_stop", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Stop(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_go_back", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GoBack(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_go_forward", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GoForward(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_can_go_back", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetCanGoBack(nint controller, out int canGoBack);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_can_go_forward", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetCanGoForward(nint controller, out int canGoForward);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_execute_script_async", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int ExecuteScriptAsync(nint controller, string script, ScriptCompletedCallback callback, nint userData);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_post_web_message_as_string", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int PostWebMessageAsString(nint controller, string message);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_post_web_message_as_json", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal static extern int PostWebMessageAsJson(nint controller, string jsonMessage);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_source", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetSource(nint controller, out nint source);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_document_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetDocumentTitle(nint controller, out nint title);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetBounds(nint controller, int x, int y, int width, int height);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetBounds(nint controller, out int x, out int y, out int width, out int height);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_is_visible", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetIsVisible(nint controller, int isVisible);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_notify_parent_window_position_changed", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int NotifyParentWindowPositionChanged(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_close", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Close(nint controller);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_zoom_factor", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetZoomFactor(nint controller, double zoomFactor);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_zoom_factor", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetZoomFactor(nint controller, out double zoomFactor);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_default_background_color", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetDefaultBackgroundColor(nint controller, uint argb);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_get_default_background_color", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetDefaultBackgroundColor(nint controller, out uint argb);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_set_root_visual_target", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetRootVisualTarget(nint controller, nint visualTarget);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_send_mouse_input", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SendMouseInput(nint controller, int eventKind, int virtualKeys, uint mouseData, int x, int y);

    [DllImport(BrowserLib, EntryPoint = "jalium_webview2_open_devtools_window", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int OpenDevToolsWindow(nint controller);
}
