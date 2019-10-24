#!/bin/bash

sleep 15

echo "Setting up the config server..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-primary:27017 <<EOF
   var cfg = {
        "_id": "rs1",
        "version": 1,
        "protocolVersion": NumberLong(1),
        configsvr: true,
		"members": [
            {
                "_id": 0,
                "host": "mongo-primary:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF

sleep 15

echo "Setting up mongo-secondary-2..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-secondary-2:27017 <<EOF
   var cfg = {
        "_id": "rs2",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-secondary-2:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



echo "Setting up mongo-secondary-3..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-secondary-3:27017 <<EOF
   var cfg = {
        "_id": "rs3",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-secondary-3:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



echo "Setting up mongo-secondary-4..."

echo SETUP.sh time now: `date +"%T" `

mongo --host mongo-secondary-4:27017 <<EOF
   var cfg = {
        "_id": "rs4",
        "version": 1,
        "protocolVersion": NumberLong(1),
		"members": [
            {
                "_id": 0,
                "host": "mongo-secondary-4:27017"
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
EOF



sleep 15

echo "Setting up the mongos server..."

mongo --host mongo-secondary-1:27017 <<EOF
   sh.addShard("rs2/mongo-secondary-2:27017");
   sh.addShard("rs3/mongo-secondary-3:27017");
   sh.addShard("rs4/mongo-secondary-4:27017");
EOF

tail -f /dev/null