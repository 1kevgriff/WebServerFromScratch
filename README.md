# WebServerFromScratch
Materials from my "Build a Web Server from Scratch" presentation

## References

CS 431/531 from Old Dominion: https://cs531-f22.github.io

RFC 7230: https://www.rfc-editor.org/rfc/rfc7230

This RFC (Hypertext Transfer Protocol (HTTP/1.1): Message Syntax and Routing)  

RFC 7231: https://www.rfc-editor.org/rfc/rfc7231

This RFC (Hypertext Transfer Protocol (HTTP/1.1): Semantics and Content)  

## Sample Requests

```http
GET / HTTP/1.1
Host: localhost:4321
Connection: keep-alive
Cache-Control: max-age=0
sec-ch-ua: "Chromium";v="124", "Microsoft Edge";v="124", "Not-A.Brand";v="99"
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: "Windows"
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Sec-Fetch-Site: none
Sec-Fetch-Mode: navigate
Sec-Fetch-User: ?1
Sec-Fetch-Dest: document
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-US,en;q=0.9
Cookie: _ga=GA1.1.2038469408.1702255094; _ga_G6TPE6V0YJ=GS1.1.1702408207.2.0.1702408207.60.0.0; __stripe_mid=2b43e276-6e52-48ad-804e-bafb6500d36cfeb9e9; sos-connection-name=; _ga_9HFQCF0EN7=GS1.1.1708699700.7.1.1708705340.0.0.0


```

```http
GET / HTTP/1.1
Content-Type: application/json
User-Agent: PostmanRuntime/7.36.0
Accept: */*
Postman-Token: c8cab5f1-ab6d-439c-a15a-c36fb5003aa2
Host: localhost:4321
Accept-Encoding: gzip, deflate, br
Connection: keep-alive
Content-Length: 26

{
    "hello": "world"
}


```

## Sample Responses

```http
HTTP/1.1 200 OK
Date: Mon, 27 Jul 2009 12:28:53 GMT
Server: Apache
Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT
ETag: "34aa387-d-1568eb00"
Accept-Ranges: bytes
Content-Length: 51
Vary: Accept-Encoding
Content-Type: text/plain

Hello World! My payload includes a trailing CRLF.

```
