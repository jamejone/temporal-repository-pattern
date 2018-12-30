#!/bin/bash

echo "Waiting for nodes to simulate network conditions and start mongod..."

sleep 15

echo "Setting up replica set..."

echo SETUP.sh time now: `date +"%T" `
mongo --host mongo-primary:27017 <<EOF
   var cfg = {
        "_id": "rs",
        "version": 1,
        "protocolVersion": NumberLong(1),
        "members": [
            {
                "_id": 0,
                "host": "mongo-primary:27017",
                "priority": 2
            },
            {
                "_id": 1,
                "host": "mongo-secondary-1:27017",
                "priority": 0,
                "tags": {
                    "secondary": "1"
                }
            },
            {
                "_id": 2,
                "host": "mongo-secondary-2:27017",
                "priority": 0,
                "tags": {
                    "secondary": "2"
                }
            }
        ]
    };
    rs.initiate(cfg, { force: true });
    rs.reconfig(cfg, { force: true });
    db.getMongo().setReadPref('secondary');
EOF

tail -f /dev/null