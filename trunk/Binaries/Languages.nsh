!insertmacro MUI_LANGUAGE "English"

;; Enable these once all relevant strings are LangString'ed (and translated)
;;!insertmacro MUI_LANGUAGE "Dutch"
;;!insertmacro MUI_LANGUAGE "Japanese"

;; Enable these as soon as multiple languages are available
;; NOTE: Make sure the uninstaller is then also set up to reuse the installation language!
;;!insertmacro MUI_RESERVEFILE_LANGDLL
;;Function .onInit
;;  !insertmacro MUI_LANGDLL_DISPLAY
;;FunctionEnd

LangString DESC_SECTION_MAIN             ${LANG_ENGLISH} "POLUtils itself."
LangString DESC_SECTION_TRANS            ${LANG_ENGLISH} "Translated resources for POLUtils (optional)."
LangString DESC_SECTION_TR_JA            ${LANG_ENGLISH} "Japanese resources for POLUtils."
LangString DESC_SECTION_TR_NL            ${LANG_ENGLISH} "Dutch resources for POLUtils."
LangString DESC_SECTION_DESKTOP_SHORTCUT ${LANG_ENGLISH} "A shortcut to POLUtils on the Desktop."
LangString DESC_SHORTCUT                 ${LANG_ENGLISH} "A collection of PlayOnline-related utilities."
LangString LOG_DOTNET_FOUND              ${LANG_ENGLISH} "Found .NET framework $DOTNET_VERSION (build $DOTNET_BUILD) (mscorlib.dll version $DOTNET_DLLVERSION)"
LangString LOG_MDX_NOT_FOUND             ${LANG_ENGLISH} "WARNING: Managed DirectX not found - no audio playback possible until it is installed."
LangString LOG_MDX_FOUND                 ${LANG_ENGLISH} "Found Managed DirectX - audio playback will be available."
LangString MB_DOTNET_NOT_FOUND           ${LANG_ENGLISH} "Version 1.1 of the Microsoft .NET Framework was not found on this system.$\r$\nYou should be able to install it using Windows Update (Windows 98 or higher required).$\r$\n$\r$\nUnable to continue this installation."
LangString MB_MDX_NOT_FOUND              ${LANG_ENGLISH} "Managed DirectX does not seem to be installed on this system$\r$\naAudio playback will not be available.$\r$\n$\r$\nAn installer for Managed DirectX is available on the web page."
LangString SITE_NAME                     ${LANG_ENGLISH} "Pebbles' Program Page"
LangString UI_RUNPROG                    ${LANG_ENGLISH} "&Run POLUtils"
