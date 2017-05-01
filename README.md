# NetworkWatchdog
An easy way to check network availability in .NET programs. Just using NetworkWatchdog.

# How To Use
1. Directly copy the code you need. Paste it into your own program.
2. Add code like this:

*For VB.NET*
```
Friend WithEvents checker As New NetworkWatchdog.NetworkWatchdog(,"http://clients3.google.com")
Public Sub NetworkchangedEvent(sender As Object, e As NetworkWatchdog.NetworkAvailability) Handles checker.NetworkAvailabilityChanged
  (Write your code here)
End Sub
```

*For C#*
```
private NetworkWatchdog.NetworkWatchdog withEventsField_checker = new NetworkWatchdog.NetworkWatchdog(, "http://clients3.google.com");
internal NetworkWatchdog.NetworkWatchdog checker {
	get { return withEventsField_checker; }
	set {
		if (withEventsField_checker != null) {
			withEventsField_checker.NetworkAvailabilityChanged -= NetworkchangedEvent;
		}
		withEventsField_checker = value;
		if (withEventsField_checker != null) {
			withEventsField_checker.NetworkAvailabilityChanged += NetworkchangedEvent;
		}
	}
}
public void NetworkchangedEvent(object sender, NetworkWatchdog.NetworkAvailability e)
{
}
```

Each time the network availability changed. The *NetworkchangedEvent* will raise. Process it properly by yourself.
