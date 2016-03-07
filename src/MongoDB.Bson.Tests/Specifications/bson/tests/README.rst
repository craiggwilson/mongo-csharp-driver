==========
BSON TESTS
==========

The YAML and JSON files in this directory tree are platform-independent tests
meant to exercise the encoding, decoding, and string operations associated with
BSON types.

Converting to JSON
==================

The tests are written in YAML because it is easier for humans to write
and read, and because YAML supports a standard comment format. Each test
is also provided in JSON format because in some languages it is easier
to parse JSON than YAML.

If you modify any test, you should modify the YAML file and then
regenerate the JSON file from it. 
	
One way to convert the files is using an online web page. I used:

http://www.json2yaml.com/

It's advertised as a JSON to YAML converter but it can be used in either direction.

Note: the yaml2json utility from npm is not capable of converting these YAML tests
because it doesn't implement the full YAML spec.

Format
======

Each test file will contain a single top-level document containing a "description" and 
or more test type arrays. Each test type array contains one or more tests abiding by
the test type. 

Test Types
----------

valid
~~~~~

"valid" tests will contain:

- "description": What is being tested.
- "subject": A hexadecimal representation of a document containing a single element.
- "string": The string value of the element.
- "extjson": The extended json format matching the subject.

A way to use these is as follows in pseudo-code:

.. code::

    var subject = DecodeFromHex(test["subject"]);

    // verify round trip
    var reencoded = EncodeToHex(subject);
    assert(reencoded == test["subject"]);

    // verify string output
    var str = subject.ToString();
    assert(str == test["string"]);
    
    if(test["extjson"]) {
        // verify decoding extjson
        // this also verifies parsing behavior
        var parsed = EncodeToHex(test["extjson"]);
        assert(parsed == test["subject"]);
    }
      
parseErrors
~~~~~~~~~~~

"parseErrors" tests will contain:

- "description": What is being tested.
- "subject": An invalid string that should fail parsing.

Use these test by attempting to parse the "subject" and ensure that
an error or exception occurs.

decodeErrors
~~~~~~~~~~~~

"decodeErrors" tests will contain:

- "description": What is being tested.
- "subject": An invalid hex string that should fail decoding.

Use these tests by attempting to decode the subject (by first turning it
into bytes) and ensure an error or exception occurs.
