OutFile "bin\Release\SystrayProxyManagerInstaller.exe"

; InstallDir "$APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
InstallDir "$APPDATA\SystrayProxyManager"

Section
	SetOutPath $INSTDIR
	WriteUninstaller $INSTDIR\uninstaller.exe
	; ExecWait "C:\Windows\System32\taskkill.exe /F /IM systrayproxymanager.exe"
	File "bin\Release\SystrayProxyManager.exe"
	File "bin\Release\Hardcodet.Wpf.TaskbarNotification.dll"
	CreateDirectory "$SMPROGRAMS\SystrayProxyManager\"
	CreateShortCut "$SMPROGRAMS\SystrayProxyManager\SystrayProxyManager.lnk" "$INSTDIR\SystrayProxyManager.exe"
	CreateShortCut "$SMPROGRAMS\SystrayProxyManager\Uninstall SystrayProxyManager.lnk" "$INSTDIR\uninstaller.exe"
	CreateShortCut "$APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\SystrayProxyManager.lnk" "$INSTDIR\SystrayProxyManager.exe"
	Delete "$INSTDIR\proxy_list.dat"
	Exec "$INSTDIR\SystrayProxyManager.exe"
SectionEnd

Section "Uninstall"
	Delete "$INSTDIR\uninstaller.exe"
	Delete "$INSTDIR\SystrayProxyManager.exe"
	Delete "$INSTDIR\Hardcodet.Wpf.TaskbarNotification.dll"
	Delete "$INSTDIR\proxy_list.dat"
	Delete "$SMPROGRAMS\SystrayProxyManager\SystrayProxyManager.lnk"
	Delete "$SMPROGRAMS\SystrayProxyManager\Uninstall SystrayProxyManager.lnk"
	RMDir "$SMPROGRAMS\SystrayProxyManager\"
	Delete "$APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\SystrayProxyManager.lnk"
	RMDir $INSTDIR
SectionEnd