/*  Developer Challenge - Word Finder - for QU Beyond Matrix

The code created was tested in LINQPad 8 using .NET 8, in Visual Studio 2022 in .NET 7 and can be found in Fiddler (.NET 9)
at the link below (private, accessible only through this link):
- https://dotnetfiddle.net/5IKH3B

In the email I attached two files, one containing the requested class, another more complete one that also contains the tests
and a version for LINQPad.

Some comments were kept in the code to highlight observations of interest.



The analysis follows:

Based on the dimensions provided on the input data, I concluded that the best strategy would be to index the Matrix, allowing
fast searches when iterating over the Wordstream.

Preprocessing the matrix: as shown below in the method header, the volume of data justifies its indexing, it was decided to use
the Trie data structure for this, all horizontal and vertical substrings were indexed. Due to the low volume of data in the matrix,
I understood that it would be better to improve the readability of the code by using Linq.

Find Method: Implemented iteration through all elements, executing the search in the Trie. In addition to synchronous execution,
multithreaded execution modes were also implemented, with options for Task (TPL) or Thread, however, their use seems unnecessary
since concurrent access to the structure that stores the Top Words cancels out the speed gains.

NOTE: It was confirmed that Threads are working correctly through tests. To reproduce these tests in a simplified way, add "Sleep(100)"
in the EvaluateWord method and adjust the Wordstream to 100 words, then run with different ThreadingMode and ThreadCount settings,
the performance gain can be observed in the processing time.

Storage of Most Frequent Words (TopWords):
To manage the most frequent words, I used a PriorityQueue and a HashSet, including a control to avoid including words below the
minimum count. The result is sorted by the most accessed. This implementation will keep only the 10 most accessed words in memory,
and together with the implemented minimum count control, it will avoid excessive maintenance in the PriorityQueue, thus with low
memory usage and high processing efficiency.

IMPORTANT: Considering the volume of data possible in the Wordstream, the search performance in the Trie, and the minimum count
control of TopWords, I understood that it would be better to process repeated words again in the Wordstream, avoiding the creation
of another queue that could consume a lot of memory.

FOR DISCUSSION: One point that I would bring to the team for discussion is the use of two structures in parallel to control TopWords.

Unit Tests: A simplified version of unit tests was included to test basic validations and support development.

Main Test: Tests for the main functionality were also implemented, including measurement of execution time. In addition to the most
basic tests, more advanced tests with up to 10 million records were performed. Eventually, not all advanced tests will be included
in the version provided, allowing for better code readability, however they will be available upon request.

About the suggested interface: considering that the suggested interface was encapsulated in a class, and because the constructor
participates in this contract, I understood that I should not use interface implementation or abstract class implementation in
respect of the provided model.

This code has been set to be case insensitive, but could easily be adjusted or parameterized.
*/