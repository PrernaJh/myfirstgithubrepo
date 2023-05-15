How to set up the database:

1. Create a cosmos database and add all collections, each with a partition key named "partitionKey"

2. Add json items manually (copy/paste valid json into cosmosDb)for first time configuration.  See data import folders

	a. Upload json for each site to sites collection in CosmosDb

	b. Upload json for each customer to customers collection

	c. Upload Job Options json for each site to jobOptions collection

	d. Upload Return Options json for each site to returnOptions collection

	e. Upload Sequences json for each site to sequences collection
		i. There are 5 sequenceTypes as of writing: 
						JOB 
						PACKAGE  
						CONTAINER 
						EVSFILE 
						ALTERNATECARRIER
						
		Note: all sequences share the same "getSequenceNumber" stored procedure
					
3. Import files by uploading them to the configured container in azure storage. Files are in data import folder, source files are also provided.

	a. Upload Bins and Bin Maps for each site

	b. Upload a Service Rules file for each customer
	
	c. Upload service rule extensions file to the serviceRuleExtensions collection 
		i. these are global for all sites
		ii. Right now there is only one file, 48states		
	
	d. Upload Rates for each customer
	
	e. Upload zip maps to the zipMaps collection
		i. LSO
		ii. OnTrac
		
	f. Upload zones file (postal zones)
		i. Global for all sites

4. Import the zipOverrides files through the Website interface (Service Management, Manage Zip Schemas)
	a. FedExHawaii zips
	b. UPS NDA Sat zips
	c. Ups DAS zips

5. Import an ASN file for a site

6.  Test
	i. Create a container
	ii.  Add a job
	iii. Test a ScanPackage call