# P2P Network Demo using gRPC and C#

A demo project that implements a basic Peer-to-Peer (P2P) network using Google gRPC and C#. This project showcases a decentralized auction system where nodes can bid on auctions hosted by other nodes.

## Features

- **Seed Node Registration**: Central bootstrapper node (`P2P.SeedNode`) that registers all peers on the network.
- **Decentralized Communication**: Peer nodes establish direct gRPC connections, enabling efficient message exchange.
- **Auction System**: Nodes can create, broadcast, and bid on auctions in a decentralized manner.
- **Shared Service Layer**: Contains all protocol services used by nodes for seamless integration.
- **Bid Management**: Each node tracks auction status, manages bids, and broadcasts bid acceptance and closure.

## Getting Started

### Prerequisites

- .NET Core 3.1+ installed
- Google gRPC tools for C#

### Setup

1. **Clone the repository:**

   ```bash
   git clone https://github.com/mutlukaplan/P2P.BidHandler.git
   cd p2p-network-demo
