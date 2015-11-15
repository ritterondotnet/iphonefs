### _[01-10-2008] - Downloads Updated_ ###


## About ##

  * First of all, Feel free to submit bugs, but I know there are lots, it is an alpha.
  * Second, this works just fine on unhacked phones, it will just show your jail root (/var/root/Media).
  * For windows obviously, though it might be ported to linux under mono.

If anyone wants to work on the source with me, and knows C#, let me know through comments or email - I would love some help.

Though the code for the iPhoneFS is GPLv3, the CIFS server implementation is not open source yet, and isn't mine, though the author keeps saying he might make open source soon. As such, it can only be distributed in binary form - that is what the Non-Free drictory is for.

Once I get the bugs worked out, it will run as a windows service, so you have the drive whenever you plug in your iPhone.



## Simple Instructions ##

unzip, run iPhoneDriveControl.exe.
  1. A messagebox will pop up, asking for the location of iTunesMobileDevice.
  1. Select the file, hit ok.
  1. The program will start.
  1. When it runs, it will find the next available drive letter.
  1. The drive letter will be printed to the Trace window.
  1. The drive letter will be mapped to the phone's filesystem.
  1. Now you can use Windows Explorer or the command line, makes no diff.


## Screenshots ##

![http://img503.imageshack.us/img503/4798/image1py7.png](http://img503.imageshack.us/img503/4798/image1py7.png)

![http://img510.imageshack.us/img510/1797/image1jc5.png](http://img510.imageshack.us/img510/1797/image1jc5.png)