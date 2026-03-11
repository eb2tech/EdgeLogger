# EdgeLogger

EdgeLogger is an exploration into more IOT fun.

There's a service designed to run on a Raspberry Pi Zero 2 W, running Pi OS. It consumes some NATS-based topics that originate from a device 
in the [Aura2](https://github.com/eb2tech/Aura2) project. There's a small amount of "edge processing" fun where this service
simply writes to a local datastore. More might be added if I can come up with an example.

There's going to be a web frontend that allows viewing of received log messages, but I'm still working out how I want it to function. That is,
a single front end that can run on the Pi and in the cloud and adjusts to whereever it is.

Of special note is development of suitable copilot-instructions.md matched with ADRs done so that both Copilot and humans have context. That's a
work in progress.

Planned features

- Web frontend, Blazor-based
- Cloud-based message processor with some kind of AI use to better understand that stuff
- Experiment with digital twins
