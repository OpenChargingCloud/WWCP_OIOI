
##Errata
  - https://docs.plugsurfing.com has a broken TLS-certificate
  - Station name and description should be multi-language
  - "street-number" in example http://docs.plugsurfing.com/en/latest/calls/station_post.html#calls-stationpost-docs should be string not number!
  - "notes" is in example http://docs.plugsurfing.com/en/latest/calls/station_post.html#calls-stationpost-docs of type BOOLEAN, but STRING according to the docs!
  - What means "is-plugin-charge"?
  - What means "is-roofed"?
  - A connector status like "out-of-service" is missing!
  - Example connector Id in http://docs.plugsurfing.com/en/latest/calls/session_post.html#calls-sessionpost-docs 'DE\*8PS\*TABCDE\*1' seems to to be a valid EVSE Id (missing "...\*E...")!
  - Unclear how to result of a SessionStart should look like. http://docs.plugsurfing.com/en/stable/calls/session_start.html seems incorrect and http://docs.plugsurfing.com/en/latest/calls/session_start.html seems also buggy.

