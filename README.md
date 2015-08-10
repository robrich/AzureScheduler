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

4. Adjust the schedules and content for starter and stopper projects.  Currently stopper is set to run at 7 am and starter is not set to run at all.


**NOTE**: If you're publishing this site to Azure, you can't use the certificate from the publishsettings file.  See [https://social.msdn.microsoft.com/Forums/vstudio/en-US/99ce89b8-17c8-464a-8135-9e18feb7d072/cant-use-publish-credentials-on-azure-websites?forum=windowsazurewebsitespreview](https://social.msdn.microsoft.com/Forums/vstudio/en-US/99ce89b8-17c8-464a-8135-9e18feb7d072/cant-use-publish-credentials-on-azure-websites?forum=windowsazurewebsitespreview) and [http://stackoverflow.com/questions/22030955/cant-create-new-schedules-from-azure-websites](http://stackoverflow.com/questions/22030955/cant-create-new-schedules-from-azure-websites).  Instead you'll need to:

1. Create a self-signed certificate in [OpenSSL](https://www.openssl.org/docs/HOWTO/certificates.txt) or in [IIS](https://technet.microsoft.com/en-us/library/Cc753127(v=WS.10).aspx) or from various [sites](http://www.selfsignedcertificate.com/).  You can use a certificate you purchased for a website.

	// http://stackoverflow.com/questions/10175812/how-to-create-a-self-signed-certificate-with-openssl
	openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes

2. [Convert](https://www.sslshopper.com/article-most-common-openssl-commands.html) it to a cer file for the Azure portal.

	// https://www.sslshopper.com/article-most-common-openssl-commands.html
	openssl x509 -outform der -in cert.pem -out cert.cer

3. [Convert](https://www.sslshopper.com/article-most-common-openssl-commands.html) it to a pfx file for this app.

	// https://www.sslshopper.com/article-most-common-openssl-commands.html
	openssl pkcs12 -export -out cert.pfx -inkey key.pem -in cert.pem

4. [Upload](https://msdn.microsoft.com/en-us/library/azure/gg551722.aspx) the cer file to each subscription in the Azure Portal.

	[https://manage.windowsazure.com/](https://manage.windowsazure.com/) -> Settings -> Management Certificates  -> Upload

4. Base-64 encode the pfx file using C# or [si](http://base64-encoding.online-domain-tools.com/)[tes](http://www.giftofspeed.com/base64-encoder/).

	// Paste this in LinqPad:
	// http://stackoverflow.com/questions/25919387/c-sharp-converting-file-into-base64string-and-back-again
	byte[] AsBytes = File.ReadAllBytes("cert.pfx");
	string AsBase64String = Convert.ToBase64String(AsBytes);
	File.WriteAllText("cert.pfx-base64-encoded.txt", AsBase64String);

**NOTE**: Azure Git Deploy [ignores](http://stackoverflow.com/questions/27158266/scheduled-azure-webjob-deployed-via-git-results-in-on-demand-job) the webjob-publish-settings.json file that contains the schedule.  Either publish through Visual Studio or create a `settings.job` file that contains the schedule.  See [http://blog.amitapple.com/post/2015/06/scheduling-azure-webjobs/#.VZWmh_lVhBc](http://blog.amitapple.com/post/2015/06/scheduling-azure-webjobs/#.VZWmh_lVhBc).  Re-deploying the webjob doesn't change the schedule, but once you have the job set as a scheduled job, you can modify the schedule through the Azure portal.
