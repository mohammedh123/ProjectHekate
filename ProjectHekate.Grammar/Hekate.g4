grammar Hekate;

/*
 * Parser Rules
 */

script
	:	(functionDeclaration|emitterUpdaterDeclaration|actionDeclaration)*
	;
	
variableDeclaration
	:	VAR NormalIdentifier ASSIGN expression
	;
	
functionDeclaration
	:	FUNCTION NormalIdentifier formalParameters functionBody
	;

emitterUpdaterDeclaration
	:	EUPDATER NormalIdentifier formalParameters updaterBody
	;

actionDeclaration
	:	ACTION NormalIdentifier formalParameters updaterBody
	;

controllerDeclaration
	:	CONTROLLER NormalIdentifier controllerBody

emitterVarDeclaration
	:	EMITTER NormalIdentifier SEMI
	;

controllerStateDeclaration
	:	NormalIdentifier LPAREN RPAREN controllerStateBody
	;

controllerStateBody
	: block
	;

functionBody
	:	blockWithReturn
	;

updaterBody
	:	block
	;

controllerBody
	:	emitterVarDeclaration
	|

block
	:	LBRACE statement* RBRACE
	;

blockWithReturn
	:	LBRACE (statement|returnStatement)* RBRACE
	;

formalParameter
	:	NormalIdentifier
	;

formalParameterList
	:	formalParameter (COMMA formalParameter)*
	;

formalParameters
	:	LPAREN formalParameterList? RPAREN
	;

// statements
statement
	:	block	# BlockStatement
	|	IF parExpression statement (ELSE statement)?		# IfStatement
	|	FOR LPAREN forInit? SEMI expression? SEMI forUpdate? RPAREN statement				# ForStatement
	|	WHILE parExpression statement						# WhileStatement
	|	BREAK SEMI				# BreakStatement
	|	CONTINUE SEMI			# ContinueStatement
	|	SEMI					# EmptyStatement
	|	VAR NormalIdentifier ASSIGN createEmitterExpression SEMI	# CreateEmitterVariableStatement
	|	VAR NormalIdentifier ASSIGN buildEmitterExpression SEMI	# BuildEmitterVariableStatement
	|	variableDeclaration SEMI							# VariableDeclarationStatement
	|	expression SEMI			# ExpressionStatement
	|	FiringFunctionName=NormalIdentifier TypeName=NormalIdentifier parExpressionList withUpdaterOption? SEMI	# FireStatement
	|	NormalIdentifier ATTACH NormalIdentifier parExpressionList withUpdaterOption? SEMI		# AttachEmitterStatement
	|	WAIT expression FRAMES SEMI		# WaitStatement
	;

returnStatement
	:	RETURN expression SEMI
	;
		
fromEmitterOption
	:	FROM NormalIdentifier
	;
	
forInit
	:	variableDeclaration
	|	expressionList
	;

forUpdate
	:	expressionList
	;	

// expressions
parExpression
	:	LPAREN expression RPAREN
	;

expressionList
	:	expression (COMMA expression)*
	;

parExpressionList
	:	LPAREN expressionList? RPAREN
	;

updaterCallExpression
	:	NormalIdentifier parExpressionList
	;

createEmitterExpression
	:	CREATE EMITTER parExpressionList withUpdaterOption?
	;

buildEmitterExpression
	:	BUILD NormalIdentifier
	;

withUpdaterOption
	:	WITH UPDATER updaterCallExpression
	;

expression
	:	LPAREN expression RPAREN	# ParenthesizedExpression
	|	Literal						# LiteralExpression
	|	NormalIdentifier			        # NormalIdentifierExpression
	|	PropertyIdentifier					# PropertyIdentifierExpression
	|	NormalIdentifier parExpressionList		# FunctionCallExpression
	|	(NormalIdentifier|PropertyIdentifier) Operator=(INC|DEC)		# PostIncDecExpression
	|	Operator=(
			SUB
		|	BANG
		)
		expression	# UnaryExpression
	|	expression
		Operator=(
			MUL 
		|	DIV
		|	MOD
		|	ADD
		|	SUB
		|	LE
		|	GE
		|	LT
		|	GT
		|	EQUAL
		|	NOTEQUAL
		|	AND
		|	OR
		)
		expression	# BinaryExpression
	|	<assoc=right> expression QUESTION expression COLON expression		# TernaryOpExpression
	|	<assoc=right> (NormalIdentifier | PropertyIdentifier) 
		Operator=(
			ASSIGN
		|	MUL_ASSIGN 
		|	DIV_ASSIGN
		|	ADD_ASSIGN
		|	SUB_ASSIGN
		)
		expression  # AssignmentExpression
	;
	
/*
 * Lexer Rules
 */

// Keywords
FUNCTION	: 'function';
CONST		: 'const';
VAR			: 'var';
FOR			: 'for';
WHILE		: 'while';
IF			: 'if';
ELSE		: 'else';
CONTINUE	: 'continue';
BREAK		: 'break';
RETURN		: 'return';

CREATE		: 'create';
EMITTER		: 'emitter';
CONTROLLER	: 'controller';
WITH		: 'with';
UPDATER		: 'updater';
ACTION		: 'action';
EUPDATER	: 'emitterUpdater';
ATTACH		: 'attach';
BUILD		: 'build';
FROM		: 'from';
WAIT		: 'wait';
FRAMES		: 'frames';

//literals
Literal
	:	FloatingPointLiteral
	|	IntegerLiteral
	;

// Integer literals
fragment
IntegerLiteral
	:	'0'
	|	NonZeroDigit Digits*
	;

fragment
FloatingPointLiteral
	:	Digits '.' Digits
	;

fragment
Digits
	:	Digit+
	;

fragment
Digit
	:	'0'
	|	NonZeroDigit
	;

fragment
NonZeroDigit
	:	[1-9]
	;
	
// Separators
LPAREN	: '(';
RPAREN	: ')';
LBRACE	: '{';
RBRACE	: '}';
SEMI	: ';';
COMMA	: ',';

// Operators
ASSIGN		: '=';
GT			: '>';
LT			: '<';
BANG		: '!';
ADD			: '+';
SUB			: '-';
MUL			: '*';
DIV			: '/';
MOD         : '%';
QUESTION	: '?';
COLON		: ':';
EQUAL		: '==';
LE			: '<=';
GE			: '>=';
NOTEQUAL	: '!=';
AND			: '&&';
OR			: '||';
INC			: '++';
DEC			: '--';
ADD_ASSIGN	: '+=';
SUB_ASSIGN	: '-=';
MUL_ASSIGN	: '*=';
DIV_ASSIGN	: '/=';

// Identifiers
NormalIdentifier
	:	Letter LetterOrDigit*
	;

PropertyIdentifier
	:	DOLLAR NormalIdentifier
	;


fragment
Letter
	:	[a-zA-Z_]
	;

fragment
LetterOrDigit
	:	[a-zA-Z0-9_]
	;

DOLLAR	: '$';

WS 
	: [ \t\r\n\u000C]+ -> skip;

COMMENT
	: '/*' .*? '*/' -> skip;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip;