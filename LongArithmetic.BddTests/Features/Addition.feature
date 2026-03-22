@bdd @addition @imperative
Feature: Addition of long positive integers
  As a financial analyst
  I want to add long positive integers without numeric overflow
  So that I can verify totals that do not fit into standard numeric types

  Background:
    Given the calculator works with positive integer strings

  Rule: Leading zeros do not change the numeric meaning of an operand
    Scenario: User adds two normalized values step by step
      Given the operands are "0000123" and "0000045"
      When I add the numbers
      Then the result should be "168"
      And the normalized result should not start with unnecessary zeros
      But the result should remain a positive integer

  Rule: Carry must propagate through every affected digit
    Scenario Outline: Addition keeps boundary transitions correct
      Given the operands are "<first>" and "<second>"
      When I add the numbers
      Then the result should be "<sum>"

      Examples:
        | first  | second | sum     |
        | 9      | 1      | 10      |
        | 99     | 1      | 100     |
        | 999999 | 1      | 1000000 |
