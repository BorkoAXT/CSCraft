@rem Gradle wrapper bootstrap - bundled by CSCraft SDK
@if "%DEBUG%"=="" @echo off
set DIRNAME=%~dp0
if defined JAVA_HOME (
    set JAVA_EXE=%JAVA_HOME%\bin\java.exe
) else (
    set JAVA_EXE=java.exe
)
"%JAVA_EXE%" -jar "%DIRNAME%gradle\wrapper\gradle-wrapper.jar" %*
