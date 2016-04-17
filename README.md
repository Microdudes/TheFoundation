# TheFoundation
Showcase für eine Device 2 Device Verbindung mit einem Azure IoT Hub

## Michael (besser gesagt Michaels Comlink)
UWP App auf einem Windows 10 Mobile das über Bluetooth mit einem Microsoft Band 2 verbunden ist. Fungiert ebenfalls als Gerät innerhalb des IoT Hubs.
Das Band sendet stetig die Herzfrequenz und das RR Interval an das IoT Hub. Vom Windows 10 Mobile wird zusätzlich noch die GPS Position abgefragt und ebenfalls mitgesendet.

## K.I.T.T.
UWP App auf dem Raspberry Pi fungiert als Gerät im IoT Hub

K.I.T.T. lauscht auf die Daten von Michael. Das RR-Interval bestimmt die Laufgeschwindigkeit Sensor LEDs. Sobald die Herzfrequenz auf über 160 Schläge pro Minute steigt, sendet K.I.T.T. eine DistressCall Nachricht an das IoT Hub.
Michael weiß daraufhin, dass K.I.T.T. zu ihm unterwegs ist.

##SEMI
In einem IoT Hub können Geräte nicht direkt miteinander Nachrichten austauschen. SEMI wird daher passen als Nachrichtenrouter zwischen Michael und K.I.T.T. eingesetzt. Nachrichten von Michael werden sofort an K.I.T.T. weitergeleitet und umgekehrt.


