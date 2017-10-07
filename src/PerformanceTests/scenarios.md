
# Scenarios

All scenarios support concurrent processing.


## Gated Publish

> Performs a continious test where a batch of messages is send via the bus without a transaction and a handler processes these in parallel. Once all messages are received it repeats this. Due to the fact that the sending is not transactional the handler will already process messages while the batch is still being send.

Seeds a small amount of events to the test endpoint. These are processed, and each received event results in the same event to be published and will be received by the same endpoint.

This quickly alternates consuming and producing of small sets of messages.

Involves only the transport unless message driven pubsub is used for subscription storage.

Note: Same as *Gated SendLocal* but for events.

## Gated SendLocal

> Performs a continuous test where a batch of messages is send via the bus without a transaction and a handler processes these in parallel. Once all messages are received it repeats this. Due to the fact that the sending is not transactional the handler will already process messages while the batch is still being send.

Seeds a small amount of commands to the test endpoint. These are processed, and each received command results in the same command to be send via SendLocal and will be received by the same endpoint.

This quickly alternates consuming and producing of small sets of messages.

Involves only the transport.

Note: Same as *Gated Publish* but for commands.

## Publish one on one

> Does a continious test where a configured set of messages are 'seeded' on the queue. For each message that is received one message will be published. This means that the sending of the message is part of the receiving context and thus part of the same transaction.
> When the test is stopped, the handler stops forwarding the message. The test continues until no new messages are received.

Produces and consumes messages simultaniously.

Involves only the transport unless message driven pubsub is used for subscription storage.

Note: Same as *Send local one on one* but for events

## Receive

> Seeds a large set of messages to the transport and after seeding completes, processes all the message with a NOOP handler.

Produces (seeds) messages first, then only consumes.

Involves only the transport.


## Saga congestion

> Seeds a set of message greater then the allowed max concurrency but share the same saga instance identifier. This WILL results in single item congestion. Persistence configurations that support pessimistic locking should not have any issues with this.
> Part of the processing operation is sending the same messages to itself to keep this running until the test stops

Produces and consumes messages simultaniously.

Involves both the transport and persistence.

Note: Same scenario as *Saga update* but guaranteed single item congestion.

## Saga initiate

> Seeds a large set of messages with unique saga instance identifiers. Processing these result in only creating new saga instances. 

Produces (seeds) messages first, then only consumes.

Involves both the transport and persistence.

## Saga update

> Seeds a set of message greater then the allowed max concurrency but have unique same saga instance identifiers. This CANNOT result in single item congestion. It can never happen that a message with the same saga identifier is processed in parallel unless it is a outbox transport operation redelivery.
> Part of the processing operation is sending the same messages to itself.

Produces and consumes messages simultaniously.

Involves both the transport and persistence.

Note: Same scenario as *Saga congestion* but guaranteed concurrenty concurrent saga processing.

## SendLocal one on one

> Does a continious test where a configured set of messages are 'seeded' on the queue. For each message that is received one message will be send. This means that the sending of the message is part of the receiving context and thus part of the same transaction.
> Then the test is stopped the handler stops forwarding the message. The test waits until no new messages are received.

Produces and consumes messages simultaniously.

Involves only the transport.

Note: Same as *Publish one on one* but for commands
