# Return YouTube Dislike API Cache

Reverse Caching Proxy for [Return YouTube Dislike](https://returnyoutubedislike.com/) backend.

## About

---

**Disclaimer:** this repository is *not* affiliated with the Return YouTube Dislike project, it is a leartning exercise for how to quickly build a load-reliefing caching proxy for an HTTP REST API.

---

This ASP.NET project acts like a reverse proxy for the Return YouTube Dislike backend and locally caches responses in a database to take load away from the backend while servicng public requests.

## Setup instructions

- Download and install [Apache CouchDB](https://couchdb.apache.org/)
  - Configure a new database `youtube-dislikes` read- and writable to the public (we assume the DB keeps running on `localhost`)
- Take a look at `appsetting.json` (should need no changes from the defaults)
- Run the project

## How to use

TBD

## 3rd party credits

- [Yet Another Reverse Proxy](https://github.com/microsoft/reverse-proxy)
- [MyCouch](https://github.com/danielwertheim/mycouch)
- [CouchDB](https://couchdb.apache.org/)
