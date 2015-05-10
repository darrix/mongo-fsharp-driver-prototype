MongoDB F# Driver Prototype
===========================

This is a prototype MongoDB driver written for F#. The goal of this
driver is to make using MongoDB from F# more natural by defining new
ways to express database/collection operations that are idiomatic to
the language.

#### Special Notes

The API and implementation are currently subject to change at any time.
You **must not** use this driver in production, as it is still under
development and is in no way supported by MongoDB, Inc.

We absolutely encourage you to experiment with it and provide us
feedback on the API, design, and implementation. Bug reports and
suggestions for improvements are welcomed, as are pull requests.

Dependencies
------------

  * F# 3.0

Building
--------

  - Simply build the `FSharpDriver-2012.sln` solution in Visual Studio, Xamarin Studio, or Mono Develop.
    You can also use the FAKE script:

      * Windows: Run _build.cmd_
      * Mono: Run _build.sh_

License
-------

[Apache v2.0](LICENSE)
