# How to reproduce the issue
- Start the mongodb container using `docker-compose up` command
- Start the Orleans server
- Start multiple Orleans clients
- Stop or kill the Orleans server
- Start the Orleans server

The clients will not receive the new events published on the stream. Note sometimes a few clients still receive events, but not all.