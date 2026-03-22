@bdd @division
Feature: Division of long positive integers
  As an accountant
  I want to divide long integers with deterministic integer truncation
  So that quotient calculations remain predictable for ledger-style processing

  Scenario Outline: Division strategies return the expected integer quotient
    Given the operands are "<dividend>" and "<divisor>"
    And the calculation method is "<method>"
    When I divide the first number by the second
    Then the result should be "<quotient>"

    Examples:
      | dividend | divisor | method  | quotient |
      | 100      | 7       | classic | 14       |
      | 100      | 7       | binary  | 14       |
      | 23       | 5       | classic | 4        |
      | 23       | 5       | binary  | 4        |

  Scenario: Binary search division and classic division agree on a representative input
    Given the operands are "98765432109876543210" and "1234567890"
    When I divide the first number by the second using "classic"
    And I divide the first number by the second using "binary"
    Then the algorithm results should match
    And the shared algorithm result should be "80000000737"
