# Description
Parser for raw bitcoin core blk/rev files into sql database. The raw transactions stored in the blk files are parsed and transformed into transactions, inputs and outputs.
A source of these files is therefore a prerequisite. However, the machine running the project is not required to an active node in the network. The files are assumed to be stored as downloaded from bitcoin core, all in the same folder.

Currently, as of February 2025, the raw files require in the order of 700 GB of storage. The fully populated sql database, with added indexes for efficient retrieval requires an additional 3 TB storage.

The notebooks first populate a table with net inflow data from the 1000 addresses with the largest holding of BTC, as reported by https://bitinfocharts.com. A transformation of the net flow across the addresses is used to generate a visualization along with past 10 years of BTC price development.
Due to the strong correlation, especially with valleys in the BTC/USD price, the flow may serve as a signal of bear run end / bull run start.
