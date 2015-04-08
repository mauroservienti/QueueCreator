# QueueCreator

Allows to create MSMQ queues as expected by a NServiceBus endpoint, V3 or V4 and V5, by default queues are created for V4 and higher.

### How to

`qc name=EndpointName` will create the following queues:

* EndpointName
* EndpointName.retries
* EndpointName.subscriptions
* EndpointName.timeouts
* EndpointName.timeoutsdispatcher

All queues are created as transactional queues. THe default security settings will be:

* `SYSTEM` -> FullControl;
* `Administrators` -> FullControl;
* `Anonymous Logon` -> WriteMessage;
* `Everyone` -> WriteMessage;
* `Everyone` -> GetQueueProperties;

To add the user running the command with full control over the created queues add the `acwu` parameter to the command line.
