Introduction
============
This is a C# library which is a port of John Walker's ENT- a pseudorandom sequence analyzer. My implementation is not 100% complete, but has all of the main aspects. Original page still online at http://www.fourmilab.ch/random/ as of 12/12/2014.

To use this ENT class, simply instantiate an EntCalc, and perform an AddSample for each byte, or use the overloads to add an entire byte array. When all samples have been added, call EndCalculation() which will return a struct with all of the results.

I have also included a simple class which you can pass in an IO stream or a filename, and it will run through the stream/file and perform the calculation, returning the struct.

Caveats
=======
Unfortunately, I don't understand all of the math behind these calculations, and the code is more or less a direct copy of his work, just ported from C++ using C# functions.

I emailed a copy to the original author, but received no reply. Since his software is in the public domain, I offer this as well, but take no real credit for it, no more than someone who translated a document from another language.


Notes
=====
I did get one reply on the Code Project post that indicated that the numbers seemed to compare favorably with the original source.

The code has not been modified in something like a decade and may need some updates to work with newer .NET.
