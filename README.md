P2P Network Demo
This project demonstrates a basic Peer-to-Peer (P2P) network using Google gRPC and C#. The core idea is to enable nodes to register with a seed node and establish communication through gRPC socket connections. The project also includes an auction mechanism where nodes can act as bidders or auction owners.

Project Overview
Seed Node: The seed node acts as the bootstrapper of the P2P network. It must be run first, as it keeps track of all nodes that join the network.
Peer Nodes: Each peer node registers itself with the seed node upon initialization, allowing the P2P network to maintain a registry of all connected nodes.
Shared Layer: A separate shared layer is implemented to handle common protocol services, which are used by all nodes.
Cache Service: Each node has a registered cache service to store auctions it owns.
How It Works
Node Registration: When a node starts, it registers with the seed node, gaining awareness of other nodes in the network.
Creating Auctions: Nodes can create auctions, which are automatically broadcasted across the network.
Bidding Process:
A node can view all auctions by connecting to the seed node, which provides information on all registered nodes.
Each node maintains a cache of its auctions, and the collective auction data can be obtained by aggregating all node caches.
Nodes can place bids on auctions created by other nodes. A node cannot bid on its own auctions.
The auction owner can close the auction at any time, broadcasting the closure to all nodes, which then remove the auction from their caches.
Broadcasting: Messages are sent through a broadcast service on the seed node, but peer-to-peer interactions occur directly between nodes during bidding.
Setup and Running the Project
Prerequisites
.NET Core 3.1 or later
gRPC tools for C#
Running the Project
Start the Seed Node: Run P2P.SeedNode as the first step. This node is required to register all subsequent peer nodes.
Start Peer Nodes: Run multiple instances of peer nodes, which will register themselves with the seed node.
Create Auctions: Use the provided functionality to create auctions on any peer node. The auction will be broadcasted across the network.
Bid on Auctions: Other nodes can view and bid on auctions. The owner of the auction can finalize the auction at any time, and the final bid will be registered.
For a demonstration, refer to the video walkthrough provided here. (Replace # with your video link)

Key Features
gRPC-based Communication: All communication between nodes and the seed node is handled via gRPC, ensuring efficient data exchange.
Decentralized Auction System: Allows for a decentralized auction system where nodes interact directly during the bidding process.
Dynamic Node Awareness: The seed node maintains an updated list of all active nodes, facilitating dynamic discovery of peers.
Bid Control: Nodes can make bids on auctions hosted by other nodes, with restrictions to prevent self-bidding.
Suggested Improvements
AuctionService Implementation: A recommended approach is to implement AuctionServiceImp, allowing nodes to act as either bidders or auction owners.
Enhanced Bid Management: Future iterations could include more complex bidding rules or support for concurrent bid handling.
Notes
All nodes, including the seed node, should be run as new instances for proper functioning.
For more details, refer to the video tutorial, which covers the setup and demonstrates how to create and manage auctions in the network.
License
This project is licensed under the MIT License.