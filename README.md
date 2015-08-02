Azure Scheduler
===============

A web app / web job for starting and stopping Azure resources via the Azure SDK

Install
-------

1. Publish this site to Azure Web Apps

2. Set AppSettings in the Azure portal (NOT the code) for each subscription from https://manage.windowsazure.com/publishsettings/index?client=powershell, read by SubscriptionRepository:

	- subscriptionId1
	- managementCert1
	- subscriptionId2
	- managementCert2
	- etc

3. Set AppSettings for site authentication in the Azure portal (NOT the code), read by HomeController:

	- LoginUsername
	- LoginPassword
