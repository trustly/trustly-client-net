= A header

== A sub-header


TODO:
* Fixa TrustlyApi, SettlementCsv -> extrahera ut till en egen klass, och använd generics istället för en map
* Ändra en del typer till managed typer istället för strängar, t.ex. Currency
* Make use of "Methods" enum, so that "Method" is no longer a string
* Remove "HandleJsonResponse" inside TrustlyApi -- or rather, rewrite so you do not manually need to know what the response type is going to be
    * Same thing goes for SendRequest -- hide this from the end user. Use Extension methods? Sounds like a good idea?