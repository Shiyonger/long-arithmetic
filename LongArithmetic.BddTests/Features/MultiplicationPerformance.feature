@bdd @multiplication @load
Feature: Multiplication algorithm selection under load
  As a performance engineer
  I want to compare classic and Karatsuba multiplication on representative datasets
  So that I can choose a reliable strategy for heavy long arithmetic calculations

  Background:
    Given the multiplication algorithms "classic" and "karatsuba" are available

  Rule: Both multiplication algorithms must always return the same product
    Scenario: Baseline comparison matches the current unit-test oracle
      Given the operands are "9999999999999999999999999999999999999999" and "8888888888888888888888888888888888888888"
      When I multiply the numbers using "classic"
      And I multiply the numbers using "karatsuba"
      Then the algorithm results should match
      And the multiplication result should satisfy the unit-test comparison oracle

  Rule: Preliminary load experiments should cover different data profiles and sizes
    Scenario Outline: Benchmark both multiplication algorithms on representative datasets
      Given multiplication inputs generated for "<profile>" data with <digits> digits per operand
      When I benchmark multiplication algorithm "classic"
      And I benchmark multiplication algorithm "karatsuba"
      Then the algorithm results should match
      And the timings should be collected for all benchmarked algorithms
      But each benchmark should finish within <maxMilliseconds> milliseconds
      And the benchmark profile should be recorded as "<profile>" with size <digits>

      Examples:
        | profile | digits | maxMilliseconds |
        | dense   | 40     | 1000            |
        | dense   | 120    | 3000            |
        | sparse  | 40     | 1000            |
        | sparse  | 120    | 3000            |

    Scenario: Preliminary experiment identifies the largest safe candidate size
      Given the candidate multiplication sizes are
        | Digits |
        | 40     |
        | 120    |
        | 240    |
      When I run the preliminary multiplication experiment for "dense" data
      Then a maximal safe multiplication size should be selected
      And the selected size should be at least 40 digits
      But the full experiment should finish within 30000 milliseconds
