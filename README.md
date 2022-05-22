2022.04.28 Franklin T9 Manager - controls a Franklin T9 (r717) Hotspot device over SSH via plink.exe and PowerShell

Made using Visual Studio 2022 (64 Bit) Community Edition, written in C#

Publish settings: Release-anycpu, Framework-dependent, net6.0-windows, Portable

Detailed documentation on the Franklin T9 (r717) 4G Hotspot https://docs.google.com/document/d/1LgYLB0sJbwAMW2VfcNoGavIJQhiYCJzsWK4onOFdSys/edit

Features include:

- lets you know if the T9 is accessible on the network (I use an Asus router to extend wifi reach/hardwire multiple devices so simply being network connected doesn't mean the T9 is connected)

- menu options to remotely reboot the T9

- links to all of the secret pages and notifications that, upon clicking provide the appropriate password for copy by clicking a notification

- link to comprehensive set up document I'm working on

Future features:

- connection (band and signal strength) and data usage info on a bubble popup

- band selection menu

- automated setup (e.g. disabling automatic update, sim unlocking, installing TTL modifier script)

- changing default passwords for hidden pages, wifi network