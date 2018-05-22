1. 
The VM was created using the VMWare Player version 6.0.3 which can be downloaded as a free download from the VMWare site. You can use the latest version of the free tool to launch it. Once you have downloaded it, you can also convert it into other formats to open it with another virtualization tool of your choice.


2. It was created using the 180 day evaluation versions Windows Server 2012 R2, SQL Server 2016 and the SharePoint Server 2016 Trial license (June 2017 CU). If you’d like to, you can enter your own keys to activate the trial/ evaluation versions. These 180 day evaluation versions should be good through Jan 29, 2018.


3. It consists of 4 hard drives, a VMWare virtual machine configuration (.vmx) file and other supporting VMWare files. All the files and hard drives are part of 11 self-extracting compressed files. The total size of the compressed files is 10.6GB (with the total size of the extracted files at about 27.5 GB). These self-extracting(executable) archive can be downloaded from the download link mentioned below.


4. The VM has 16 GB RAM allocated to it by default (controlled by the settings in the ".vmx" file or via the virtual machine settings in the VMWare player). You can reduce or increase the RAM to suite your needs, although, 12 GB is the minimum required and 16 GB is what I will recommend for this VM.


5. It has the Classic Shell start menu installed from http://classicshell.sourceforge.net/ so you can get the old Windows start menu in addition to the new Windows "Start screen". This also results in Windows booting up to the desktop instead of the new Windows start screen. Although, if you’d like to, you can get to the Start screen by clicking the Shift+Windows button combination. You can also configure Classic Shell so Windows boots up to the start screen instead, if you so desire.


6. It has the gmsp2016.dev as the Active Directory domain and has the apps service configured to use the apps.gmsp2016.dev app domain.


7. Once you start the VM, Windows will auto login to the SP_Admin account. This "gmsp2016\sp_admin" account was the account used to install SharePoint and should be used to access central admin. pass@word1 is the common password for all the accounts in the VM.


8. The virtual machine additionally has the following accounts created and used for various services:

    a. SP_Farm: The farm account

    b. SP_CacheSuperReader: The object cache read access account

    c. SP_CacheSuperUser: The object cache full access account

    d. SP_ExcelUser: Account used for excel services (not yet configured on the VM)

    e. SP_PeffPointUser: Account used for performance point services (not yet configured on the VM)


    f. SP_PortalAppPool: Account for the content web application pools


    g. SP_ProfilesAppPool: Web app pool for the MySites web application (not yet configured on the VM)


    h. SP_ProfileSync: The user profile synchronization account


    i. SP_SearchContent: The default content access account for the Search Service Application


    j. SP_SearchService: Account to run the SharePoint Search "Windows Service


    k. SP_Services: Service Applications  App Pool account


    l. SP_VisioUser: Account used for Visio services (not yet configured on this VM)
SQL_Admin: Used to Install the SQL Server
SQL_Services: SQL service account for the MSSQLServer & SQLServerAgent services


	You can read more on different accounts for SharePoint in the following articles: http://technet.microsoft.com/en-us/library/cc263445(v=office.15).aspx & http://www.toddklindt.com/blog/Lists/Posts/Post.aspx?ID=39


9. If the VM does not start after you have downloaded it, ensure that the number of files you have downloaded, the total size of the archived files and the total size of the extracted files matches what's specified above.

Instructions for extracting the VM: You will need to download all the .rar and .exe files to your machine and then run the "SP2016SP1.part01.exe" executable to unpack the virtual machine hard drives. After ensuring that you have the latest version of the VMWare Player installed, you can then double-click the "SP2016.vmx" file to run the virtual machine.

Instructions to activate the licenses are mentioned in my blog post. The comments in the previous post also contains answers to some common questions that others have asked.

Please however do feel free to drop me a line if you face any issues OR even if you find the VM useful! :)
Gaurav Mahajan
Twitter: @mahajang (https://twitter.com/mahajang)
LinkedIn: https://www.linkedin.com/in/imgauravmahajan
Blog: http://gauravmahajan.net