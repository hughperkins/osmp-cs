Helperclasses are in theory fairly small, standalone components that can be 
reused from project to project.

They differ from BasicTypes in that they generally dont created new types of things,
but rather provide helper functions to make it easier to work with things that already exist.

For example, Arguments takes the passed in string[] args array and parses it into
Named and UnNamed lists.
