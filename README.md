# Siren
Siren currently is an implementation of a particular domain model for Linear Playout Broadcast Automation.

Eventually, I'd like for Siren to become a service-based event driven Linear Playout Broadcast Automation system which expresses this model.

## Model Documentation
[Siren Domain Model](https://docs.google.com/document/d/1essfdaSvxf9eSKffh3dk0FKroHp4fk4vMZXy155610U/edit?usp=sharing)

## Model Implementation Challenges
### Transmission List to Device List Uses Anonoymous Types to pass data
One of the things I did as a pass to get data into a structure that could be transmitted between the Device and the Transmission List was to building the different strategies into event data json, by just taking the member variables, assigning them to anonymous data structures and letting json serialisation do the rest. This was quick and easy, and also deals with the fact that we could add any derived strategies and still have it generate the right data. But it looks a little messy and adhoc to me.

Thoughts about this? Potentially something that could be improved.

## Development
- Please make sure all pull requests into master pass all unit tests!
- Pull requests should be reviewed by at least one other team member

### Contributing
The best thing to do is to browse the Project descriptions. Currently the focus is on the Siren Alpha, but we'll also need some implementation of a Playout Device Driver as well, that the core can then interact with.

The Siren Alpha description contains an order list of headline items which have been organised into milestones in the issues list. So it's worth having a read of the milestone descriptions as well. 

Then just get stuck in somewhere! If you navigate to the Siren Alpha project page, I've identified tickets that I think would be good to work on soon and put those in the backlog, but any open ticket is fair game. And if you see something that's missing feel free to add or adjust a ticket, would be good if you could tag the others in the group on the ticket if you do create or adjust the scope of a ticket.

The codebase is totally maleable, if you see something needs changing, go ahead and change it and highlight it if it seems pertinent. Try to work to the domain model, and definitely highlight if something in the model isn't fitting.
