# NetCore-ColorRest
Provar de fer un servei REST senzill amb NetCore i SQLite ... Un alumne deia que era difícil ...

La idea és fer un servei REST que respongi a l'adreça /api/colors i que tregui les dades d'una base 
de dades SQLite (l'empleno amb dades per defecte perquè la idea és provar com es poden fer testos
funcionals)

Funcionament
---------------------
S'inicia el programa de la forma habitual:

    $ cd colorsRest
    $ dotnet run
    Hosting environment: Production
    Content root path: /home/xavier/work-random/programming/dotnet/NetCore-ColorRest/colorsRest
    Now listening on: http://localhost:5000
    Now listening on: https://localhost:5001
    Application started. Press Ctrl+C to shut down.

Es pot provar el funcionament amb el navegador o bé Curl o Httpie:

    $ curl http://localhost:5000/api/colors
    [{"id":1,"nom":"vermell","rgb":"#FF0000"},{"id":2,"nom":"verd","rgb":"#00FF00"},{"id":3,"nom":"blau","rgb":"#0000FF"}]

Només s'ofereixen dos punts de connexió: 

| URL           | Resultat                                    |
| ------------- | ------------------------------------------- |
| /api/colors   | Retorna tots els colors de la base de dades |
| /api/colors/1 | Retorna el color amb l'id especificat       |


Tests funcionals
--------------------------
La idea de tot plegat era aconseguir fer els tests funcionals amb Net Core 2.1 (els unitaris són relativament semblants als de
Java). Faig servir xunit.

L'execució és idèntica a executar un programa: 

    $ cd tests/functional/
    $ dotnet test
    $ dotnet test
    Build started, please wait...
    Build completed.

    Test run for /home/xavier/work-random/programming/dotnet/NetCore-ColorRest/tests/functional/bin/Debug/netcoreapp2.1/functional.dll(.NETCoreApp,Version=v2.1)
    Microsoft (R) Test Execution Command Line Tool Version 15.7.0
    Copyright (c) Microsoft Corporation.  All rights reserved.

    Starting test execution, please wait...

    Total tests: 6. Passed: 6. Failed: 0. Skipped: 0.
    Test Run Successful.
    Test execution time: 6,1351 Seconds

La idea és iniciar un servidor de proves 
i executar-hi els testos. He creat una classe a part *Helpers.cs* per entrar les dades de prova en una base de
dades en memòria.

Tant els testos com el programa són senzillets però els he fet per comprovar com funciona tot plegat. Crec que si ho 
compliqués més potser no quedaria tant clar com fer els testos funcionals.


    
