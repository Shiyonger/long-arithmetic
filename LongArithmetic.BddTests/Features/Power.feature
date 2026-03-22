@bdd @power
Feature: Exponentiation of long integers
  As a research engineer
  I want to raise long integers to non-negative powers with alternative strategies
  So that I can calculate large deterministic powers efficiently

  Scenario Outline: Exponentiation supports binary and iterative methods
    Given the base is "<baseValue>" and the exponent is <exponent>
    And the calculation method is "<method>"
    When I raise the base to the exponent
    Then the result should be "<power>"

    Examples:
      | baseValue | exponent | method    | power            |
      | 2         | 0        | binary    | 1                |
      | 2         | 10       | binary    | 1024             |
      | 2         | 10       | iterative | 1024             |
      | 3         | 5        | iterative | 243              |

  Scenario: Binary exponentiation and iterative exponentiation agree
    Given the base is "3" and the exponent is 15
    When I raise the base to the exponent using "binary"
    And I raise the base to the exponent using "iterative"
    Then the algorithm results should match
    And the shared algorithm result should be "14348907"
