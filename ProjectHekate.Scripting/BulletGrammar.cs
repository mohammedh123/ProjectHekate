using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Irony.Parsing;

namespace ProjectHekate.Scripting
{
    [Language("BulletGrammar", "1.0", "A grammar for describing bullet patterns")]
    public class BulletGrammar : Grammar
    {
        TerminalSet _skipTokensInPreview = new TerminalSet();

        public BulletGrammar()
        {
            // lexical structure
            var stringLiteral = TerminalFactory.CreateCSharpString("StringLiteral");
            var charLiteral = TerminalFactory.CreateCSharpChar("CharLiteral");
            var number = TerminalFactory.CreateCSharpNumber("Number");
            var identifier = TerminalFactory.CreateCSharpIdentifier("Identifier");

            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(singleLineComment);

            // symbols
            var semi = ToTerm(";", "semicolon");
            var semiOpt = new NonTerminal("semi?") {Rule = Empty | semi};
            var dot = ToTerm(".", "dot");
            var comma = ToTerm(",", "comma");
            var commaOpt = new NonTerminal("comma?") {Rule = Empty | comma};
            var commasOpt = new NonTerminal("commasOpt");
            commasOpt.Rule = MakeStarRule(commasOpt, null, comma);
            var lbr = ToTerm("{");
            var rbr = ToTerm("}");
            var lpar = ToTerm("(");
            var rpar = ToTerm(")");

            // basics
            var typeRef = new NonTerminal("typeRef");
            var typeRefList = new NonTerminal("typeRefList");
            var builtinType = new NonTerminal("builtinType");
            var typeOrVoid = new NonTerminal("typeOrVoid");
            var identifierOrBuiltIn = new NonTerminal("identifierOrBuiltIn");
            var integralType = new NonTerminal("integralType");

            // expressions
            var argument = new NonTerminal("argument");
            var argumentList = new NonTerminal("argumentList");
            var argumentListOpt = new NonTerminal("argumentListOpt");
            var argumentListPar = new NonTerminal("argumentListPar");
            var argumentListParOpt = new NonTerminal("argumentListParOpt");
            var expression = new NonTerminal("expression", "expression");
            var expressionList = new NonTerminal("expressionList", "expressionList");
            var expressionOpt = new NonTerminal("expressionOpt");
            var binOpExpression = new NonTerminal("binOpExpression");
            var unaryOperator = new NonTerminal("unaryOperator");
            var assignmentOperator = new NonTerminal("assignmentOperator");
            var primaryExpression = new NonTerminal("primaryExpression");
            var unaryExpression = new NonTerminal("unaryExpression");
            var preIncrDecrExpression = new NonTerminal("preIncrDecrExpression");
            var postIncrDecrExpression = new NonTerminal("postIncrDecrExpression");
            var literal = new NonTerminal("literal");
            var incrOrDecr = new NonTerminal("incrOrDecr");
            var incrOrDecrOpt = new NonTerminal("incrOrDecrOpt");
            var parenthesizedExpression = new NonTerminal("parenthesizedExpression");
            var memberAccess = new NonTerminal("memberAccess");
            var memberAccessSegment = new NonTerminal("memberAccessSegment");
            var memberAccessSegmentsOpt = new NonTerminal("memberAccessSegmentsOpt");
            var binOp = new NonTerminal("binOp", "operator symbol");

            var elemInitializer = new NonTerminal("elemInitializer");
            var elemInitializerList = new NonTerminal("elemInitializerList");
            var elemInitializerListExt = new NonTerminal("elemInitializerExt");
            var initializerValue = new NonTerminal("initializerValue");

            // statements
            var statement = new NonTerminal("statement", "statement");
            var statementList = new NonTerminal("statementList");
            var statementListOpt = new NonTerminal("statementListOpt");
            var declarationStatement = new NonTerminal("declarationStatement");
            var embeddedStatement = new NonTerminal("embeddedStatement");
            var selectionStatement = new NonTerminal("selectionStatement");
            var iterationStatement = new NonTerminal("iterationStatement");
            var statementExpression = new NonTerminal("statementExpression");
            var statementExpressionList = new NonTerminal("statementExpressionList");
            var localVariableDeclaration = new NonTerminal("localVariableDeclaration");
            var localConstDeclaration = new NonTerminal("localConstDeclaration");
            var localVariableType = new NonTerminal("localVariableType");
            var localVariableDeclarator = new NonTerminal("localVariableDeclarator");
            var localVariableDeclarators = new NonTerminal("localVariableDeclarators");
            var initializeDeclaration = new NonTerminal("initializeDeclaration");
            var block = new NonTerminal("block");
            var ifStatement = new NonTerminal("ifStatement");
            var elseClauseOpt = new NonTerminal("elseClauseOpt");
            var whileStatement = new NonTerminal("whileStatement");
            var forStatement = new NonTerminal("forStatement");
            var forInitializerOpt = new NonTerminal("forInitializerOpt");
            var forConditionOpt = new NonTerminal("forConditionOpt");
            var forIteratorOpt = new NonTerminal("forIteratorOpt");
            var breakStatement = new NonTerminal("breakStatement");
            var continueStatement = new NonTerminal("continueStatement");
            var returnStatement = new NonTerminal("returnStatement");

            // top-level objects and stuff
            var emitterDeclarationList = new NonTerminal("emitterDeclarationList");
            var emitterDeclaration = new NonTerminal("emitterDeclaration");
            var qualifiedIdentifier = new NonTerminal("qualifiedIdentifier");
            var emitterBody = new NonTerminal("emitterBody");
            var emitterMemberDeclaration = new NonTerminal("emitterMemberDeclaration");
            var emitterMemberDeclarations = new NonTerminal("emitterMemberDeclarations");
            var stateDeclaration = new NonTerminal("stateDeclaration");
            var stateDeclarations = new NonTerminal("stateDeclarations");
            var stateBody = new NonTerminal("stateBody");

            // emitter stuff
            var memberDeclaration = new NonTerminal("memberDeclaration");
            var memberDeclarationsOpt = new NonTerminal("memberDeclarationsOpt");
            var constantDeclaration = new NonTerminal("constantDeclaration");
            var constantDeclarator = new NonTerminal("constantDeclarator");
            var constantDeclarators = new NonTerminal("constantDeclarators");
            var fieldDeclaration = new NonTerminal("fieldDeclaration");
            var functionDeclaration = new NonTerminal("functionDeclaration");
            var variableDeclarator = new NonTerminal("variableDeclarator");
            var variableDeclarators = new NonTerminal("variableDeclarators");
            var functionBody = new NonTerminal("methodBody");
            var formalParameterList = new NonTerminal("formalParameterList");
            var formalParameterListPar = new NonTerminal("formalParameterListPar");
            var fixedParameter = new NonTerminal("fixedParameter");
            var fixedParameters = new NonTerminal("fixedParameters");

            // operators
            RegisterOperators(1, "||");
            RegisterOperators(2, "&&");
            RegisterOperators(3, "|");
            RegisterOperators(4, "^");
            RegisterOperators(5, "&");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(7, "<", ">", "<=", ">=");
            RegisterOperators(8, "<<", ">>");
            RegisterOperators(9, "+", "-");
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(-3, "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            RegisterOperators(-2, "?");

            MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");

            MarkTransient(emitterMemberDeclaration, memberDeclaration, statement, embeddedStatement, expression, literal, binOp,
                primaryExpression, expression);

            AddTermsReportGroup("assignment", "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            AddTermsReportGroup("typename", "bool", "float", "string", "int", "char");
            AddTermsReportGroup("statement", "if", "while", "for", "continue", "return", "break");
            AddTermsReportGroup("type declaration", "emitter", "state");
            AddTermsReportGroup("constant", number, stringLiteral, charLiteral);
            AddTermsReportGroup("constant", "true", "false", "null");
            AddTermsReportGroup("initialize", "Initialize");
            AddTermsReportGroup("unary operator", "+", "-", "!", "~");

            AddToNoReportGroup(comma, semi);
            AddToNoReportGroup("const", "++", "--", "this", "{", "}", "[");

            // types
            typeRef.Rule = typeOrVoid;
            typeRefList.Rule = MakePlusRule(typeRefList, comma, typeRef);
            integralType.Rule = ToTerm("int") | "char";
            typeOrVoid.Rule = identifierOrBuiltIn | "void";
            builtinType.Rule = integralType | "bool" | "float" | "string";
            identifierOrBuiltIn.Rule = identifier | builtinType;

            // expressions
            expression.Rule = binOpExpression | primaryExpression;
            expressionOpt.Rule = Empty | expression;
            expressionList.Rule = MakePlusRule(expressionList, comma, expression);

            argument.Rule = expression;
            argumentList.Rule = MakePlusRule(argumentList, comma, argument);
            argumentListOpt.Rule = Empty | argumentList;

            unaryOperator.Rule = ToTerm("+") | "-" | "!" | "~" | "*";
            assignmentOperator.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | ">>=";

            binOpExpression.Rule = expression + binOp + expression;

            unaryExpression.Rule = unaryOperator + primaryExpression;
            primaryExpression.Rule =
                literal
                | unaryExpression
                | parenthesizedExpression
                | memberAccess
                | preIncrDecrExpression
                | postIncrDecrExpression;

            literal.Rule = number | stringLiteral | charLiteral | "true" | "false";
            parenthesizedExpression.Rule = lpar + expression + rpar;
            preIncrDecrExpression.Rule = incrOrDecr + memberAccess;
            postIncrDecrExpression.Rule = memberAccess + incrOrDecr;

            memberAccess.Rule = identifierOrBuiltIn + memberAccessSegmentsOpt;
            memberAccessSegmentsOpt.Rule = MakeStarRule(memberAccessSegmentsOpt, null, memberAccessSegment);
            memberAccessSegment.Rule = dot + identifier
                                       | argumentListPar;
            elemInitializer.Rule = initializerValue | identifier + "=" + initializerValue;
            elemInitializerList.Rule = MakePlusRule(elemInitializerList, comma, elemInitializer);
            elemInitializerListExt.Rule = Empty | elemInitializerList + commaOpt;
            initializerValue.Rule = expression;
            argumentListPar.Rule = lpar + argumentListOpt + rpar;
            argumentListParOpt.Rule = Empty | argumentListPar;

            initializerValue.Rule = expression;
            binOp.Rule = ToTerm("<") | "||" | "&&" | "|" | "^" | "&" | "==" | "!=" | ">" | "<=" | ">=" | "<<" | ">>" | "+" | "-" | "*" | "/" |
                         "%" | "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | ">>=";

            // statements
            statement.Rule = declarationStatement | embeddedStatement;
            statement.ErrorRule = SyntaxError + semi;
            statementList.Rule = MakeListRule(statementList, null, statement);
            statementListOpt.Rule = Empty | statementList;

            declarationStatement.Rule = localVariableDeclaration + semi | localConstDeclaration + semi;
            localVariableDeclaration.Rule = localVariableType + localVariableDeclarators;
            localVariableType.Rule = memberAccess;
            localVariableDeclarator.Rule = identifier | identifier + "=" + initializerValue;
            localVariableDeclarators.Rule = MakePlusRule(localVariableDeclarators, comma, localVariableDeclarator);
            localConstDeclaration.Rule = "const" + typeRef + constantDeclarator;

            embeddedStatement.Rule = block | semi | statementExpression + semi | selectionStatement | iterationStatement;
            block.Rule = lbr + statementListOpt + rbr;

            selectionStatement.Rule = ifStatement;
            ifStatement.Rule = ToTerm("if") + lpar + expression + rpar + embeddedStatement + elseClauseOpt;
            elseClauseOpt.Rule = Empty | PreferShiftHere() + "else" + embeddedStatement;

            iterationStatement.Rule = whileStatement | forStatement;
            whileStatement.Rule = "while" + parenthesizedExpression + embeddedStatement;
            forStatement.Rule = "for" + lpar + forInitializerOpt + semi + forConditionOpt + semi + forIteratorOpt + rpar + embeddedStatement;
            forInitializerOpt.Rule = Empty | localVariableDeclaration | statementExpressionList;
            forConditionOpt.Rule = Empty | expression;
            forIteratorOpt.Rule = Empty | statementExpressionList;
            breakStatement.Rule = "break" + semi;
            continueStatement.Rule = "continue" + semi;
            returnStatement.Rule = "return" + expressionOpt + semi;

            statementExpression.Rule = memberAccess | memberAccess + assignmentOperator + expression | preIncrDecrExpression |
                                       postIncrDecrExpression;
            statementExpressionList.Rule = MakePlusRule(statementExpressionList, comma, statementExpression);
            incrOrDecrOpt.Rule = Empty | ToTerm("++") | "--";
            incrOrDecr.Rule = ToTerm("++") | ToTerm("--");

            Root = emitterDeclarationList;
            emitterDeclarationList.Rule = MakePlusRule(emitterDeclarationList, null, emitterDeclaration);
            emitterDeclaration.Rule = "emitter" + qualifiedIdentifier + emitterBody + semiOpt;
            qualifiedIdentifier.Rule = MakePlusRule(qualifiedIdentifier, dot, identifier);
            emitterBody.Rule = "{" + emitterMemberDeclarations + initializeDeclaration + emitterMemberDeclarations + "}";
            emitterMemberDeclaration.Rule = memberDeclaration | stateDeclaration;
            emitterMemberDeclarations.Rule = MakeStarRule(emitterMemberDeclarations, null, emitterMemberDeclaration);
            initializeDeclaration.Rule = "Initialize" + block;
            stateDeclarations.Rule = MakePlusRule(stateDeclarations, null, stateDeclaration);
            stateDeclaration.Rule = "state" + identifier + stateBody;
            stateBody.Rule = lbr + memberDeclarationsOpt + rbr;

            memberDeclaration.Rule = constantDeclaration | fieldDeclaration | functionDeclaration;
            memberDeclaration.ErrorRule = SyntaxError + ";" | SyntaxError + "}";
            memberDeclarationsOpt.Rule = MakeStarRule(memberDeclarationsOpt, null, memberDeclaration);
            constantDeclaration.Rule = "const" + typeRef + constantDeclarators + semi;
            constantDeclarator.Rule = identifier + "=" + expression;
            constantDeclarators.Rule = MakePlusRule(constantDeclarators, comma, constantDeclarator);
            fieldDeclaration.Rule = builtinType + variableDeclarators + semi;
            variableDeclarator.Rule = identifier | identifier + "=" + elemInitializer;
            variableDeclarators.Rule = MakePlusRule(variableDeclarators, comma, variableDeclarator);

            functionDeclaration.Rule = "function" + typeRef + identifierOrBuiltIn + formalParameterListPar + functionBody;
            formalParameterList.Rule = fixedParameters;
            formalParameterListPar.Rule = lpar + rpar | lpar + formalParameterList + rpar;
            fixedParameter.Rule = typeRef + identifier;
            fixedParameters.Rule = MakePlusRule(fixedParameters, comma, fixedParameter);
            functionBody.Rule = block | semi;

            //Prepare term set for conflict resolution
            _skipTokensInPreview.UnionWith(new Terminal[] { dot, identifier, comma, ToTerm("::"), comma, ToTerm("["), ToTerm("]") });
        }
    }
}
