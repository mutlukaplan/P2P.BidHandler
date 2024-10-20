# P2P Network Demo using gRPC and C#

A demo project that implements a basic Peer-to-Peer (P2P) network using Google gRPC and C#. This project showcases a decentralized auction system where nodes can bid on auctions hosted by other nodes.

## Features

- **Seed Node Registration**: Central bootstrapper node (`P2P.SeedNode`) that registers all peers on the network.
- **Decentralized Communication**: Peer nodes establish direct gRPC connections, enabling efficient message exchange.
- **Auction System**: Nodes can create, broadcast, and bid on auctions in a decentralized manner.
- **Shared Service Layer**: Contains all protocol services used by nodes for seamless integration.
- **Bid Management**: Each node tracks auction status, manages bids, and broadcasts bid acceptance and closure.

## How It Works

- **Seed Node**: `P2P.SeedNode` serves as the bootstrapper node where all peers register themselves upon initialization. It maintains knowledge of all nodes connected with gRPC socket connections.
- **Shared Layer**: A separate shared layer implements all protocol services, enabling nodes to communicate using a common interface.
- **Cache Service**: Each node has a cache service that stores any auctions it creates.
- **Auction and Bidding Process**:
  - Nodes can create auctions, which are broadcasted across the network.
  - When a node wants to view all auctions, it connects to the seed server to retrieve a list of registered nodes.
  - Nodes maintain the latest bid status for each auction.
  - If a client wants to accept a specific bid, it sends a message to the network indicating that the bid is closed. The auction is then removed from the cache of all nodes.
  - Nodes are prohibited from bidding on auctions they create.

## Getting Started

### Prerequisites

- .NET 6.0+ installed
- Google gRPC tools for C#

### Setup

1. **Clone the repository:**

   ```bash
   git clone https://github.com/mutlukaplan/P2P.BidHandler.git
   cd p2p-network-demo

   ### Run the Seed Node

To run the Seed Node (bootstrapper), use the following command:

2. **Run seed nood first**

```bash
dotnet run --project P2P.SeedNode


3. **Run other nodes **

```bash
dotnet run --project P2P.Node1
dotnet run --project P2P.Node2
dotnet run --project P2P.Node3


## Running Example Scenario

### Example Scenario

1. **Client#1** opens auction: sell `a box of apple` for `300`.
2. **Client#2** opens auction: sell `a box of banana` for `350`.
2. **Client#3** opens auction: sell `a box of avocado` for `400`.
3. **Client#1** bids `400` for Client#1's `a box of apple`.
4. **Client#3** bids `600` for the same.
6. **Client#1** finalizes auction, informing all about the sale to **Client#3** at `600`.

