# Siren
Siren is a service-based event driven Linear Playout Broadcast Automation system

## Model Implementation Challenges
### Disconnect Between Transmission List Events and Playout Events
I encountered a challenge when try to convert from a Channel List Event (which just references an existing Transmission Event) into a Playout List Event. This is because a Playout List Event is intended to be the actual properties of an event as it's intended to run. But depending on the strategies in the Transmission Events this could be entirely different sets of properties. We can't just link the Transmission Events or original strategies, because how we'd still need to expose every possible property through the interfaces of those strategies.

My current solution here is that the strategy interfaces will support a generateStrategyData function, which can be combined into an event data json document. And the Playout List Event can then work on this abstract data. This is effectively the propertiesXML in Marina. This may also work for UIs and List Translation etc. in the future. But we may need different data, as the strategyData may include generated values (and may not include the values initially used to generate those values), and the event definition data would contain just the initial values.

Potentially we could go one step further and then create classes which represent the expected data in the document. We could then parse the json document back into a concrete object which we can perform operations on. The aim of this being that we want to leave the data in an abstract form for as little time as possible.

Please let me know if you have any better ideas for this!

## Development
- Please make sure all pull requests into master pass all unit tests!