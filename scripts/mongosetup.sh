#!/bin/bash

sleep 15

echo "Setting up the config server..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-config-server:27017 <<EOF
   var cfg = {
        "_id": "rs_config",
        "version": 1,
        "protocolVersion": NumberLong(1),
        configsvr: true,
		"members": [
            {
                "_id": 0,
                "host": "mongo-config-server:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF

echo "Setting up mongo-replica-set-1..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-replica-set-1:27017 <<EOF
   var cfg = {
        "_id": "rs1",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-replica-set-1:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



echo "Setting up mongo-replica-set-2..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-replica-set-2:27017 <<EOF
   var cfg = {
        "_id": "rs2",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-replica-set-2:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



echo "Setting up mongo-replica-set-3..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-replica-set-3:27017 <<EOF
   var cfg = {
        "_id": "rs3",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-replica-set-3:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



sleep 15

echo "Setting up the mongo shard server..."

mongo --host mongo-shard-server:27017 <<EOF
   sh.addShard("rs1/mongo-replica-set-1:27017");
   sh.addShard("rs2/mongo-replica-set-2:27017");
   sh.addShard("rs3/mongo-replica-set-3:27017");
EOF

echo "Done!"

tail -f /dev/null