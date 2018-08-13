# NetCore-ColorRest

Provar de fer un servei REST senzill amb NetCore i SQLite ... Un alumne deia que era difícil ...

La idea és fer un servei REST que respongi a l'adreça /api/colors i que tregui les dades d'una base
de dades SQLite (l'empleno amb dades per defecte perquè la idea és provar com es poden fer testos
funcionals)

## Funcionament

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

    $ http post http://localhost:5000/api/colors Nom="negre" Rgb="#000000"

    HTTP/1.1 202 Accepted
    Content-Length: 0
    Date: Mon, 30 Jul 2018 08:48:44 GMT
    Server: Kestrel

Només s'ofereixen dos punts de connexió:

| URL             | Mètode | Resultat                                    |
| --------------- | ------ | ------------------------------------------- |
| /api/colors     | GET    | Retorna tots els colors de la base de dades |
| /api/colors/1   | GET    | Retorna el color amb l'id especificat       |
| /api/colors/nom | GET    | Retorna el color amb el nom especificat     |
| /api/colors     | POST   | Afegeix el color a la BDD                   |
| /api/colors/1   | DELETE | Esborra el color de la BDD                  |

Tant el POST com el DELETE necessiten autenticació enviant en la petició un token JWT que es pot obtenir registrant un usuari o bé fent login

| URL                 | Mètode | Resultat                                                                      |
| ------------------- | ------ | ----------------------------------------------------------------------------- |
| /api/Users/Register | POST   | Registra un usuari que ha d'enviar Email i Password. Retorna el token         |
| /api/Users/Login    | POST   | Permet que un usuari s'identifiqui (Email i la Contrasenya). Retorna el token |

Exemple de registre d'un usuari amb Httpie: 

    $ http POST http://localhost:5000/api/User/Register email=xavier@localhost Password="Alls0Seves!"
    {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJtZUBsb2Nhb...s"
    }

Després aquest token és el que servirà per poder crear i elimiar colors ... 

    http  --auth-type=jwt --auth="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJtZUBsb2Nhb...s" 
          POST http://localhost:5000/api/colors Nom="beix" Rgb="#F5F5DC" -v

  
El programa hauria de fer algunes comprovacions més, retornar errors de més tipus, i totes aquestes coses, però com que l'objectiu només era provar els testos (i ara JWT) per ara ho deixo així

## Tests funcionals

La idea de tot plegat era aconseguir fer els tests funcionals amb Net Core 2.1 (els unitaris són relativament semblants als de
Java, tot i que també n'he fet algun i alguna cosa em sembla rareta). Faig servir xunit (que sembla que és el que es porta ara).

L'execució és idèntica a executar un programa:

    $ cd tests/functional/
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

La idea és iniciar un servidor de proves i executar-hi els testos. He creat una classe a part _Helpers.cs_ per entrar les dades de prova en una base de dades en memòria.

Tant els testos com el programa són senzillets però els he fet per comprovar com funciona tot plegat. 
