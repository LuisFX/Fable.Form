﻿module Tests.Base_Optional

open Mocha
open Fable.Form
open Fable.Form.Simple

type Values = { Email: string; Password: string }

let emailError = "Email should contains the '@' symbol"

let emailField =
    Form.emailField
        {
            Parser =
                fun value ->
                    if value.Contains "@" then
                        Ok value
                    else
                        Error emailError
            Value = fun values -> values.Email
            Update = fun newValue values -> { values with Email = newValue }
            Error = always None
            Attributes =
                {
                    Label = "Email"
                    Placeholder = "Type your email"
                    HtmlAttributes = []
                }
        }

let passwordError = "The password should have at least 8 characters"

let passwordField: Form.Form<Values, string, obj> =
    Form.passwordField
        {
            Parser =
                fun value ->
                    if String.length value >= 8 then
                        Ok value
                    else
                        Error passwordError
            Value = fun values -> values.Password
            Update = fun newValue values -> { values with Password = newValue }
            Error = always None
            Attributes =
                {
                    Label = "Password"
                    Placeholder = "Type your password"
                    HtmlAttributes = []
                }
        }

let form =
    Base.succeed (fun x y -> (x, y))
    |> Base.append emailField
    |> Base.append passwordField
    |> Base.optional

let fill = Base.fill form

let validValues =
    {
        Email = "test@mail.com"
        Password = "123456789"
    }

let partiallyValidValues =
    {
        Email = "test@mail.com"
        Password = ""
    }

let invalidValues =
    {
        Email = "mail.com"
        Password = "1234"
    }

let emptyValues = { Email = ""; Password = "" }

describe
    "Base.optional"
    (fun () ->

        describe
            "when filled with empty values"
            (fun () ->

                it
                    "contains no field errors"
                    (fun () ->
                        let filledForm = fill emptyValues

                        let fieldErrors = filledForm.Fields |> List.map (fun field -> field.Error)

                        Assert.deepStrictEqual (fieldErrors, [ None; None ])
                    )

                it
                    "produces None"
                    (fun () ->
                        let filledForm = fill emptyValues

                        Assert.deepStrictEqual (filledForm.Result, Ok None)
                    )

            )

        describe
            "when filled with valid values"
            (fun () ->

                it
                    "contains no field errors"
                    (fun () ->
                        let filledForm = fill validValues

                        let fieldErrors = filledForm.Fields |> List.map (fun field -> field.Error)

                        Assert.deepStrictEqual (fieldErrors, [ None; None ])
                    )

                it
                    "results in the correct output"
                    (fun () ->
                        let filledForm = fill validValues

                        Assert.deepStrictEqual (filledForm.Result, Ok(Some("test@mail.com", "123456789")))
                    )

            )

        describe
            "when partially filled"
            (fun () ->

                it
                    "results in required field errors"
                    (fun () ->
                        let filledForm = fill partiallyValidValues

                        Assert.deepStrictEqual (filledForm.Result, Error(Error.RequiredFieldIsEmpty, []))
                    )

            )

        describe
            "when filled with invalid values"
            (fun () ->

                it
                    "contains the first error of each field"
                    (fun () ->
                        let filledForm = fill invalidValues

                        let fieldErrors = filledForm.Fields |> List.map (fun field -> field.Error)

                        Assert.deepStrictEqual (
                            fieldErrors,
                            [
                                Some(Error.ValidationFailed emailError)
                                Some(Error.ValidationFailed passwordError)
                            ]
                        )
                    )

                it
                    "results in a non-empty list with the errors of the fields"
                    (fun () ->
                        let filledForm = fill invalidValues

                        Assert.deepStrictEqual (
                            filledForm.Result,
                            Error(Error.ValidationFailed emailError, [ Error.ValidationFailed passwordError ])
                        )
                    )

            )

    )
