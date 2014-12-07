# ProjectHekate

Simply put, ProjectHekate is a shoot-em-up (henceforth known as a shmup) engine/proof-of-concept written in C#.

The goal is to have a performant shmup engine that supports scripting object behavior.

## Background

One of my favorite type of games is the shmup (also known as bullet hell/curtain). As the name suggests, a typical boss fight in a shmup consists of dodging a plethora of bullets flying across the screen while simultaneously trying to take down the boss. I've always enjoyed seeing the intricate designs used for bullet patterns and seeing how far the developer can take the concept of what a shmup is, while still retaining the core mechanics.

Since learning how to program, it's always been a stretch goal of mine to create a fully-functioning shmup engine. From my first year of university (2009), I've made countless prototypes and have learned much from the myriad of iterations I've gone through. ProjectHekate is the culmination - or, at the very least, a half decent bullet scripting engine - of those years of research and prototyping.

A couple years ago, I stumbled upon [BulletScript](https://code.google.com/p/bulletscript/) (no longer maintained, it seems) and used it in my C++ shmup iterations. It served me well. The creator had written a scripting language for use with the engine - I did my fair share of bughunting and helped fix some aberrant behavior here and there. I learned a lot recently (while writing my own language) from this codebase, hence the shoutout.

## Technical Stuff

ProjectHekate is written in **C#** and targets **.NET 4**.

The graphics library being used here is [**SFML**](http://www.sfml-dev.org/download/sfml.net/) (.NET bindings).

The scripting language of Hekate was written using **ANTLR4** (using [antlr4cs](https://github.com/tunnelvisionlabs/antlr4cs)). It was my first time writing a language (and using ANTLR, while I'm at it), but I did take some random course in university about language design. 

Some random features of the virtual machine and scripting language:
- The virtual machine is stack-based with a custom instruction set.
- All variables are floating point.
- Custom properties (denoted with a `$PropertyName`) can be defined.
- Global constants can be defined.
- There is specialized syntax for firing bullets using a firing method and also attaching an action to a bullet (that's the whole point of this language, really).

## Installation

You'll most likely need to follow the instructions on the [antlr4cs Github page](https://github.com/tunnelvisionlabs/antlr4cs) for getting your environment ready to deal with ANTLR, if you haven't already done so.

I believe this only compiles properly for x64, but replacing the SFML dlls with their x86 counterparts and changing the target platform to x86 should work.

## Screenshots/GIFs/WebMs/Samples of Scripts

_Note: I am constantly iterating over the grammar, so don't expect the scripting language to reflect what you see here._

Here's a sample of how the first near-feature-complete iteration (ability to fire bullets and script them) looks:

```
action SomeRandomShit(someShit)
{
    $Angle += PI_180*someShit;
    wait(0);
}

action UpdateBullet()
{
    var angle = 0;
    for(var i = 0; i < 30; i++) {
        angle = i*2*PI/180;
        var sprite = i % 2 == 0 ? 2 : 3;

        fire bullet($X, $Y, $Angle + angle, $Speed, sprite) with updater SomeRandomShit(i);
        wait(0);
    }

    wait(600);
}
```

What does that script look like? As follows:

![script ss](http://i.imgur.com/7mz0C6w.gif)

_Function calls work!_

![function calls woo](http://i.imgur.com/F6eLkFD.gif)

The following are from earlier iterations (before adding a scripting language, I did everything in C# code and the bullets simply had an update function).

_Rotating emitters orbiting a rotating and moving emitter! Curved lasers too? Why not?_

![rotating thingy](http://i.imgur.com/I4sXfkx.gif)

_and an even earlier version:_ [earlier version of rotating thingy](http://a.pomf.se/oezvsg.webm)

_more curved laser goodness_

![curvy goodness](http://i.imgur.com/k0Xynb2.gif)

_bullet systems orbiting a bullet_
![mm bullet systems](http://i.imgur.com/saBSDo4.gif)



That's all for now. As I add more features, and if I stumble upon (read: testing with random data) any more interesting patterns, I'll update this last section of the README.
