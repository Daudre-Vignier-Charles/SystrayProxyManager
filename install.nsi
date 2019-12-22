!include LogicLib.nsh
OutFile "bin\Release\SystrayProxyManagerInstaller.exe"

; InstallDir "$APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
InstallDir "$APPDATA\SystrayProxyManager"

Section
	Var /GLOBAL running
	testrun:
		DetailPrint "Checking running version"
		ClearErrors
		ExecWait "cmd /c 'C:\Windows\System32\qprocess.exe systrayproxymanager'" $running
		MessageBox MB_OK $running
		StrCmp $running "" run endtestrun
		run:
			DetailPrint "Running version found"
			MessageBox MB_OKCANCEL "Please close older versions of SystrayProxyManager." IDOK testrun IDCANCEL end
			Goto testrun
	endtestrun:
	DetailPrint "Running version not found"
	SetOutPath $INSTDIR
	WriteUninstaller $INSTDIR\uninstaller.exe
	; ExecWait "C:\Windows\System32\taskkill.exe /F /IM systrayproxymanager.exe"
	File "bin\Release\SystrayProxyManager.exe"
	File "bin\Release\Hardcodet.Wpf.TaskbarNotification.dll"
	CreateDirectory "$SMPROGRAMS\SystrayProxyManager\"
	CreateShortCut "$SMPROGRAMS\SystrayProxyManager\SystrayProxyManager.lnk" "$INSTDIR\SystrayProxyManager.exe"
	CreateShortCut "$SMPROGRAMS\SystrayProxyManager\Uninstall SystrayProxyManager.lnk" "$INSTDIR\uninstaller.exe"
	MessageBox MB_YESNO "Would you want to launch SystrayProxyManager at startup ?" IDYES startup IDNO next
		startup:
			CreateShortCut "$APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\SystrayProxyManager.lnk" "$INSTDIR\SystrayProxyManager.exe"
			Goto next
		next:
	Delete "$INSTDIR\proxy_list.dat"
	MessageBox MB_YESNO "Would you want to launch SystrayProxyManager now ?" IDYES launch
		launch:
			Exec "$INSTDIR\SystrayProxyManager.exe"
	end:
			
SectionEnd

Section "Uninstall"
	DetailPrint "Checking running version"
	testrun:
		ExecWait "QPROCESS systrayproxymanager>NUL" $running
		StrCmp $running "0" 0 endtestrun
			DetailPrint "Running version found"
			MessageBox MB_OK "Please close SystrayProxyManager before uninstalling it."
			Goto testrun
	endtestrun:
	DetailPrint "Running version not found"
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