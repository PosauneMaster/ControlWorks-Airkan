
@echo %DATE% %TIME%

@echo Begin install
%1\ControlWorks.Services.exe install --autostart -description %2

@echo Install complete