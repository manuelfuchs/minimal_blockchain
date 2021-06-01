# Minimal Blockchain

This implementation is my solution for an Blockchains assignment of my masters degree.
The goal was to implement a minimal version of a blockchain which does not include a client application.

## Prerequisites

* .NET 6 (used version: Preview 4)

## Instructions

To start a single node directly on the host machine execute `make run`.

To start a simulated blockchain execute `make cluster`, which uses *docker compose* to create two nodes. If you want to register nodes with other nodes, use the host name in the docker network like *node_2*.