packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -output:coverage.xml -target:"packages\xunit.runner.console.2.1.0\tools\xunit.console.exe" -targetargs:"xFunc.Tests\bin\Release\xFunc.Tests.dll -nologo -noshadow" -filter:"+[xFunc.*]* -[xFunc.Tests*]* -[xFunc.*]*.Resource -[xFunc.*]*Exception"
packages\ReportGenerator.2.5.1\tools\ReportGenerator.exe coverage.xml .\coverage
pause