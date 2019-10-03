# Siren
Siren currently is an implementation of a particular domain model for Linear Playout Broadcast Automation.

Eventually, I'd like for Siren to become a service-based event driven Linear Playout Broadcast Automation system which expresses this model.

## Model Implementation Challenges
### Disconnect Between Transmission List Events and Playout Events
I encountered a challenge when try to convert from a Channel List Event (which just references an existing Transmission Event) into a Playout List Event. This is because a Playout List Event is intended to be the actual properties of an event as it's intended to run. But depending on the strategies in the Transmission Events this could be entirely different sets of properties. We can't just link the Transmission Events or original strategies, because how we'd still need to expose every possible property through the interfaces of those strategies.

My current solution here is that the strategy interfaces will support a generateStrategyData function, which can be combined into an event data json document. And the Playout List Event can then work on this abstract data. This is effectively the propertiesXML in Marina. This may also work for UIs and List Translation etc. in the future. But we may need different data, as the strategyData may include generated values (and may not include the values initially used to generate those values), and the event definition data would contain just the initial values.

Potentially we could go one step further and then create classes which represent the expected data in the document. We could then parse the json document back into a concrete object which we can perform operations on. The aim of this being that we want to leave the data in an abstract form for as little time as possible.

We could also just pass around JObjects, rather than strings. As to do validation on those strings, we have to reconstruct them into JObjects anyway. However, that does tie the data to a particular data format internally (much like Marina is tied to XML).

Please let me know if you have any better ideas for this!

## Development
- Please make sure all pull requests into master pass all unit tests!

### To Do
- Now that we can translate a Transmission event to a JSON data string/object with the TransmissionEventTranslationService, we can add that to the PlayoutListEvent
- We should update the PlayoutListGenerationServiceTest to cover the expected json data we'd get back from the playout list event [Done]
- We can then implement PlayoutListGenerationService to pass the tests. It should create PlayoutListEvents from the event data, and then assign them to a list hashed on the device it's intended for
- (We may want to add a PlayoutListEventTranslationService which does this, but that could be overkill to be honest. We have all the event data already.)
- We need to update the Program (our Application Layer) to take the PlayoutList Dictionary and then deliver it to Devices
- We probably want to put a device onto it's own thread so that it could simulate playout, which means we then need to start using a time source abstraction and stuff like that
- Ideally we then throw a "front end" onto it of another thread which can take user input as commands and query the domain objects
- Handle Event State Updates
