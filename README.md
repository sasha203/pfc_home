# PFC Home Assignment ReadME #

### SQL (PostgreSQL server) ###

* instanceID: main-server

* Location: us-west1
* Zone: any 

* Machine type and storage:
	** 1 Core 
	** 3.75GB 
		
		
* Storage: 
	** SSD
	** 10GB (minimum) 

* Auto backups and high availability:
	** No automated backups are required for assignment 
	


### Bucket for files ###

* Name: pfc-file-bucket
* Where to store your data - Multi-region
* Default storage  class - standard
* Access to objects - fine-grained
* Encryption - Google-Managed key



### Cache Redis DB ###
* link to db: https://app.redislabs.com/#/bdbs
* db name: pfc-redis-db
* protocol: Redis


### Key ring ###
* key ring name: pfc-keyring
* key ring location: us-west1

##### key used#####
* key name: Npfc-key1
* keytype: symmetric enc/dec (1key)




### CronJobs###
https://www.setcronjob.com/signup