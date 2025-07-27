\# 🖤 UmbraLang



\*\*UmbraLang\*\* – \*"The scripting language born in the shadows."\*  

Eine minimalistische, shadow-inspirierte Scripting Language mit Terminal-Stil und Fokus auf Dateiinteraktionen und funktionale Kontrolle.



!\[UmbraLang Logo](logo.png)



---



\## 🌑 Was ist UmbraLang?



UmbraLang ist eine selbst entwickelte Skriptsprache, die mit einfachen Textbefehlen arbeitet und besonders für Automatisierung, einfache Tools und Konsole-basierte Programme geeignet ist.



Merkmale:



\- Klar definierte Syntax (inspired by C \& Shell)

\- Kompatibel mit Windows (.NET-basiert)

\- Unterstützt Variablen, Bedingungen, Schleifen, Funktionen und I/O

\- Fokus auf Klarheit, Geschwindigkeit und stilvollem Shadow-Stil



---



\## 🚀 Features



✅ `black("text")` – Ausgabe auf Konsole  

✅ `storeInput("var")` – Benutzereingabe speichern  

✅ `white name = "Max"` – Variable setzen  

✅ `code myFunc() { ... }` – Funktionen definieren  

✅ `readFile`, `writeFile`, `timestamp()`, `exists(...)`, u. v. m.  

✅ Shadow-Effekte mit `shadow(0.5)` oder `waitShadow()`



---



\## 📦 Datei-Endungen



UmbraLang-Skripte enden auf:



\- `.umbra` \*(Standard)\*

\- `.ul` \*(Optional kurz)\*



---



\## 🧪 Beispiel



```umbra

black("Willkommen in UmbraLang")

storeInput("name")

black("Hallo, " + name + "!")



